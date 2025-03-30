using AuthNuget.Proxies;
using AuthNuget.Proxies.Impl;
using AuthNuget.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static AuthNuget.Proxies.Impl.AuthServiceProxy;

namespace AuthNuget.Registration;

public static class PfeSecureHost
{
    internal static Func<Uri, ILogger, IAuthServiceProxy> AuthServiceProxyFactory { get; set; } = (uri, logger) => new AuthServiceProxy(uri, logger);

    public static IHostBuilder Create<TStartup>(string[] args) where TStartup : class
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string? authServerUrl = configuration["ServiceUrls:AuthServer"];

        var logger = loggerFactory.CreateLogger("Startup");

        if (string.IsNullOrWhiteSpace(authServerUrl))
        {
            logger.LogError("Auth server url is not set");
         
            throw new Exception("Auth server url is not set");
        }

        var certs = new
        {
            Path = configuration["Certificate:Path"],
            Password = configuration["Certificate:Password"]
        };

        if (certs == null)
        {
            logger.LogError("Certificate is not set");

            throw new Exception("Certificate is not set");
        }

        IAuthServiceProxy authProxy = AuthServiceProxyFactory(new Uri(authServerUrl), logger);

        ServerPublicKey publicKey = authProxy.GetPublicKey().Result;

        RsaPublicKeySecurityKeyConverter.Instance = new RsaPublicKeySecurityKeyConverter(publicKey.PublicKey);

        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>()
                    .ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(8081,
                            listenOptions =>
                            {
                                listenOptions.UseHttps(certs.Path, certs.Password);
                                listenOptions.UseConnectionLogging();
                            });
                    });
            });
    }
}