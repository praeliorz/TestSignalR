using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TestSignalR.Web.Services;

namespace TestSignalR.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var useHttps = Configuration.GetSection("AppSettings").GetValue(key: "UseHttps", defaultValue: true);

            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddControllersWithViews().AddNewtonsoftJson().AddSessionStateTempDataProvider().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);
            if (useHttps)
            {
                services.AddHsts(options =>
                {
                    options.IncludeSubDomains = false;
                    options.MaxAge = TimeSpan.FromSeconds(Configuration.GetSection("AppSettings").GetValue<int>("HstsMaxAgeInSeconds"));
                    options.Preload = true;
                });
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 443;
                });
            }
            services.AddMemoryCache();
            services.AddRazorPages();
            services.AddSession(options =>
            {
                var sessionSettings = Configuration.GetSection("AppSettings").GetSection("Session");

                options.Cookie.HttpOnly = sessionSettings.GetValue<bool>("HttpOnly");
                options.Cookie.IsEssential = sessionSettings.GetValue<bool>("IsEssential");
                options.Cookie.SameSite = (SameSiteMode)sessionSettings.GetValue<int>("SameSite");
                options.Cookie.SecurePolicy = (CookieSecurePolicy)sessionSettings.GetValue<int>("SecurePolicy");
                options.IdleTimeout = TimeSpan.FromSeconds(sessionSettings.GetValue<double>("IdleTimeoutInSeconds"));
            });
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.AreaViewLocationFormats.Clear();
                options.AreaViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });

            // Register application services.
            services.AddSingleton<TestService>();
            services.AddHostedService<BackgroundServiceStarter<TestService>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseStatusCodePages(async context =>
                {
                    if (context.HttpContext.Response.StatusCode == 401)
                        await context.HttpContext.Response.WriteAsync("Unauthorized");
                    else
                        context.HttpContext.Response.Redirect($"/Error/{context.HttpContext.Response.StatusCode}");
                });
            }

            /*** ORDER IS CRITICAL ***/
            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                // Attribute routing.
                endpoints.MapControllers();
                // SignalR.
                endpoints.MapHub<ChatHub>("/chathub");
                // Razor pages.
                endpoints.MapRazorPages();
            });
        }
    }
}