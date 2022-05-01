using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PluginStatsServer.Stats;

namespace PluginStatsServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            AssemblyLoadContext.Default.Unloading += (context) =>
            {
                StatsServer.Close();
            };
            AppDomain.CurrentDomain.ProcessExit += (obj, e) =>
            {
                StatsServer.Close();
            };
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddLogging(
            builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddConsole();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStopping.Register(() =>
            {
                StatsServer.Close();
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Any");
            }
            StatsServer.Load();
            app.Use(async (context, next) =>
            {
                string path = context.Request.Path.Value.ToLowerFast();
                if (path.StartsWith("/plugins/") && path != "/plugins/viewplugin")
                {
                    context.Items["plugin_id"] = path[("/plugins/".Length)..];
                    context.Request.Path = "/Plugins/ViewPlugin";
                    Console.WriteLine($"REDIR TO VIEW");
                }
                await next();
            });
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    string path = context.Request.Path.Value.ToLowerFast();
                    if (!path.StartsWith("/error/"))
                    {
                        context.Request.Path = "/Error/Error404";
                        await next();
                    }
                }
            });
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
