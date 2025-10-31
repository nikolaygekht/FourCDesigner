using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gehtsoft.FourCDesigner.Middleware
{
    /// <summary>
    /// Middleware that processes Server Side Include (SSI) directives in HTML files and variable substitutions in HTML and JavaScript files.
    /// Supports the <!--#include file="path" --> directive (HTML only) and variables: $(app-version), $(external-prefix).
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
            // Only process GET requests for HTML and JavaScript files
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

                    // Ensure all data is written to the memory stream
                    await context.Response.Body.FlushAsync();

                    // Check if this is HTML or JavaScript content
                    bool isHtml = context.Response.ContentType != null &&
                                  context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);

                    bool isJavaScript = context.Response.ContentType != null &&
                                       (context.Response.ContentType.Contains("application/javascript", StringComparison.OrdinalIgnoreCase) ||
                                        context.Response.ContentType.Contains("text/javascript", StringComparison.OrdinalIgnoreCase));

                    // Process HTML or JavaScript content
                    if ((isHtml || isJavaScript) && memoryStream.Length > 0)
                    {
                        // Read the response
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        string responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                        string processedBody;

                        if (isHtml)
                        {
                            // Process SSI includes (only for HTML)
                            processedBody = ProcessIncludes(responseBody);
                            // Process variables
                            processedBody = ProcessVariables(processedBody);
                        }
                        else
                        {
                            // For JavaScript, only process variables (no SSI includes)
                            processedBody = ProcessVariables(responseBody);
                        }

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
        /// Replaces $(external-prefix) with the configured external prefix.
        /// </summary>
        /// <param name="html">The HTML content to process.</param>
        /// <returns>The processed HTML with variables replaced.</returns>
        private string ProcessVariables(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // Replace $(app-version) with the configured version
            html = html.Replace("$(app-version)", _config.AppVersion);

            // Replace $(external-prefix) with the configured external prefix
            html = html.Replace("$(external-prefix)", _config.ExternalPrefix);

            return html;
        }
    }
}
