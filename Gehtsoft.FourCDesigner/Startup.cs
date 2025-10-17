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
            services.AddAutoMapper(cfg => { }, typeof(Startup));
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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
                await next();
            });

            app.UseCors("AllowOrigin");

            // Add SSI middleware (must be before UseStaticFiles)
            app.UseMiddleware<SsiMiddleware>();

            app.UseStaticFiles();
            app.UseRouting();

            // Add rate limiter middleware (must be after UseRouting and before UseEndpoints)
            app.UseRateLimiter();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
