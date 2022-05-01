using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PluginStatsServer
{
    public class Program
    {
        public static IHost CurrentHost;

        public static void Main(string[] args)
        {
            SpecialTools.Internationalize();
            CurrentHost = CreateHostBuilder(args).Build();
            CurrentHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
        }
    }
}
