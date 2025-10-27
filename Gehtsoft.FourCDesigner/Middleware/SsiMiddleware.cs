using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gehtsoft.FourCDesigner.Middleware
{
    /// <summary>
    /// Middleware that processes Server Side Include (SSI) directives in HTML files.
    /// Supports the <!--#include file="path" --> directive and $(app-version) variable.
    /// </summary>
    public class SsiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _webRootPath;
        private readonly ISsiMiddlewareConfig _config;
        private static readonly Regex _includeRegex = new Regex(
            @"<!--\s*#include\s+file\s*=\s*[""']([^""']+)[""']\s*-->",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public SsiMiddleware(RequestDelegate next, IWebHostEnvironment env, ISsiMiddlewareConfig config)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _next = next;
            _webRootPath = env.WebRootPath;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process GET requests for HTML files
            if (!context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Capture the original response body stream
            Stream originalBodyStream = context.Response.Body;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Replace response body with memory stream
                    context.Response.Body = memoryStream;

                    // Call the next middleware
                    await _next(context);

                    // Only process HTML content
                    if (context.Response.ContentType != null &&
                        context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        // Read the response
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        string responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                        // Process SSI includes
                        string processedBody = ProcessIncludes(responseBody);

                        // Process variables
                        processedBody = ProcessVariables(processedBody);

                        // Write processed content to original stream
                        context.Response.ContentLength = null; // Clear content length as it may change
                        memoryStream.SetLength(0);
                        await using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
                        {
                            await writer.WriteAsync(processedBody);
                            await writer.FlushAsync();
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(originalBodyStream);
                    }
                    else
                    {
                        // Not HTML, just copy the response as-is
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(originalBodyStream);
                    }
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        /// <summary>
        /// Processes SSI include directives in the HTML content.
        /// </summary>
        /// <param name="html">The HTML content to process.</param>
        /// <returns>The processed HTML with includes resolved.</returns>
        private string ProcessIncludes(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            return _includeRegex.Replace(html, match =>
            {
                string includePath = match.Groups[1].Value;

                // Security: Prevent directory traversal attacks
                if (includePath.Contains("..", StringComparison.Ordinal) ||
                    Path.IsPathRooted(includePath))
                {
                    return $"<!-- SSI Error: Invalid include path '{includePath}' -->";
                }

                // Construct full file path
                string fullPath = Path.Combine(_webRootPath, includePath.TrimStart('/'));

                // Check if file exists
                if (!File.Exists(fullPath))
                {
                    return $"<!-- SSI Error: File not found '{includePath}' -->";
                }

                try
                {
                    // Read and return the included file content
                    string content = File.ReadAllText(fullPath);

                    // Recursively process includes in the included file
                    return ProcessIncludes(content);
                }
                catch (Exception ex)
                {
                    return $"<!-- SSI Error: Failed to read '{includePath}': {ex.Message} -->";
                }
            });
        }

        /// <summary>
        /// Processes variable substitutions in the HTML content.
        /// Replaces $(app-version) with the configured application version.
        /// </summary>
        /// <param name="html">The HTML content to process.</param>
        /// <returns>The processed HTML with variables replaced.</returns>
        private string ProcessVariables(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // Replace $(app-version) with the configured version
            html = html.Replace("$(app-version)", _config.AppVersion);

            return html;
        }
    }
}
