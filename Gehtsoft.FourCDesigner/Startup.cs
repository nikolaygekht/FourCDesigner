using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Gehtsoft.FourCDesigner.Dao;
using Gehtsoft.FourCDesigner.Logic.User;
using Gehtsoft.FourCDesigner.Logic.Session;
using Gehtsoft.FourCDesigner.Logic.Email;
using Gehtsoft.FourCDesigner.Logic.Token;
using Gehtsoft.FourCDesigner.Middleware;
using Gehtsoft.FourCDesigner.Middleware.Throttling;
using Gehtsoft.FourCDesigner.Utils;
using Gehtsoft.FourCDesigner.Logic.Config;
using Gehtsoft.FourCDesigner.Logic.AI;
using Gehtsoft.FourCDesigner.Logic.Plan;

namespace Gehtsoft.FourCDesigner
{
    public class Startup
    {
        public IWebHostEnvironment CurrentEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            CurrentEnvironment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    options.AddPolicy("AllowOrigin", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
                });
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IWebHostEnvironment>(CurrentEnvironment);

            // Register system configuration
            services.AddSystemConfig();

            // Register SSI middleware services
            services.AddSsiMiddleware();

            // Register message services (localization)
            services.AddMessages();

            // Register DAO services (database, connection factory, DAOs)
            services.AddDaoServices();

            // Register token services (token generation and validation)
            services.AddTokenServices();

            // Register user services (configuration, validators, controllers)
            services.AddUserServices();

            // Register session services (session management)
            services.AddSessionServices();

            // Register email services (email queue and background sender)
            services.AddEmailServices();

            // Register throttling services (rate limiting)
            services.AddThrottling();

            // Register AI services (configurations, drivers, and factory)
            services.AddAIServices();

            // Register plan services (prompt factory, formatter, AI controller)
            services.AddPlanServices();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISystemConfig systemConfig)
        {
            // Configure forwarded headers for proxy scenarios (must be early in pipeline)
            // This allows X-Forwarded-For and X-Forwarded-Proto headers to be read properly
            var forwardedHeadersOptions = new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            };
            // Allow any proxy (for development and testing)
            // In production, you should configure specific known networks/proxies
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            // Configure path base for reverse proxy scenarios (e.g., nginx with /4c prefix)
            // This must be first in the pipeline to properly handle the path base
            if (!string.IsNullOrEmpty(systemConfig.ExternalPrefix))
            {
                app.UsePathBase(systemConfig.ExternalPrefix);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // Note: HTTPS redirection and HSTS are disabled because this application
            // is designed to run behind a reverse proxy (nginx) that handles HTTPS termination

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";

                // Relax CSP for Testing environment to allow Playwright EvaluateAsync and WaitForFunctionAsync
                // In production, use strict CSP without unsafe-eval
                if (env.IsEnvironment("Testing"))
                {
                    context.Response.Headers["Content-Security-Policy"] = "default-src 'self' 'unsafe-eval'";
                }
                else
                {
                    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
                }

                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
                await next();
            });

            app.UseCors("AllowOrigin");

            // Add SSI middleware (must be before UseStaticFiles)
            app.UseMiddleware<SsiMiddleware>();

            // Enable default files (index.html) before static files
            // This allows the root URL to serve index.html by default
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
