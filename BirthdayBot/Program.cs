using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RapidBots;
using System;
using System.Net;
using System.Security.Authentication;

namespace BirthdayBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(co =>
                    {
                        co.SslProtocols = SslProtocols.Tls12;
                    });
                    var pfxPath = GetPfxPath();
                    if (System.IO.File.Exists(pfxPath))
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(pfxPath, "Bn98rnQBS");
                        });
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps(pfxPath, "Bn98rnQBS");
                        });
                    }
                    else
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps();
                        });
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps();
                        });
                    }

                });
            });
        
        private static string GetPfxPath()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
            var rapidBotsOptions = new RapidBotsOptions();
            config.GetSection(nameof(RapidBotsOptions)).Bind(rapidBotsOptions);
            return Environment.GetEnvironmentVariable("SslCertificatePFX") ?? rapidBotsOptions.SslCertificatePFX;
        }
    }
}
