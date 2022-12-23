using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using RapidBots;
using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

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
                    var aspPfxPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");

                    if (pfxPath != null && System.IO.File.Exists(pfxPath))
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(pfxPath, "Bn98rnQBS");
                        });
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps(pfxPath, "Bn98rnQBS");
                        });
                    }
                    else if(aspPfxPath != null && System.IO.File.Exists(aspPfxPath))
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(aspPfxPath, Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password"));
                        });
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps(aspPfxPath, Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password"));
                        });
                    }
                    else
                    {
                        options.Listen(IPAddress.Any, 443, listenOptions =>
                        {
                            listenOptions.UseHttps(FindMatchingCertificateBySubject("localhost"));
                        });
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps(FindMatchingCertificateBySubject("localhost"));
                        });
                    }

                });
            });

        private static X509Certificate2 FindMatchingCertificateBySubject(string subjectCommonName)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                var certCollection = store.Certificates;
                var matchingCerts = new X509Certificate2Collection();

                foreach (var enumeratedCert in certCollection)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(subjectCommonName, enumeratedCert.GetNameInfo(X509NameType.SimpleName, forIssuer: false))
                      && DateTime.Now < enumeratedCert.NotAfter
                      && DateTime.Now >= enumeratedCert.NotBefore)
                    {
                        matchingCerts.Add(enumeratedCert);
                    }
                }

                if (matchingCerts.Count == 0)
                {
                    throw new Exception($"Could not find a match for a certificate with subject 'CN={subjectCommonName}'.");
                }

                return matchingCerts[0];
            }
        }

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
