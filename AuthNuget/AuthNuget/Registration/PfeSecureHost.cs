using AuthNuget.Proxies;
using AuthNuget.Proxies.Impl;
using AuthNuget.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using static AuthNuget.Proxies.Impl.AuthServiceProxy;

namespace AuthNuget.Registration;

public static class PfeSecureHost
{
    internal static Func<Uri, ILogger, IAuthServiceProxy> AuthServiceProxyFactory { get; set; } = (uri, logger) => new AuthServiceProxy(uri, logger);

    public static IHostBuilder Create<TStartup>(string[] args, string authServerPublicKey = "") where TStartup : class
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<TStartup>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        if (string.IsNullOrWhiteSpace(authServerPublicKey))
        {
            string? authServerUrl = configuration["ServiceUrls:AuthServer"];

            if (string.IsNullOrWhiteSpace(authServerUrl))
            {
                logger.LogError("Auth server url is not set");

                throw new Exception("Auth server url is not set");
            }

            IAuthServiceProxy authProxy = AuthServiceProxyFactory(new Uri(authServerUrl), logger);

            ServerPublicKey publicKey = authProxy.GetPublicKey().Result;

            authServerPublicKey = publicKey.PublicKey;
        }

        RsaPublicKeySecurityKeyConverter.Instance = new RsaPublicKeySecurityKeyConverter(authServerPublicKey);

        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = $"{assembly.GetName().Name}.localhost.pfx";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null) throw new Exception("Certificate resource not found.");

        byte[] buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);

        X509Certificate2 certificate = new X509Certificate2(buffer, "secret");

        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>()
                    .ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(8081,
                            listenOptions =>
                            {
                                listenOptions.UseHttps(certificate);
                                listenOptions.UseConnectionLogging();
                            });
                    });
            });
    }
}