using System.Security.Claims;
using AuthNuget.Http;
using AuthNuget.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthNuget.Registration
{
    public static class AuthRegistration
    {
        public static void RegisterPfeAuthorization(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger("JwtAuth");

                            logger.LogError(context.Exception, "JWT Authentication failed.");
                            return Task.CompletedTask;
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = RsaPublicKeySecurityKeyConverter.Instance?.RsaSecurityKey ?? throw new Exception("RSA key not found"),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        RoleClaimType = ClaimTypes.Role,
                        ValidIssuer = "auth",
                        ValidAudience = "pfe",
                    };
                });

           services.AddCors(options =>
           {
               options.AddDefaultPolicy(
                   corsPolicyBuilder =>
                   {
                       corsPolicyBuilder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                   });
           });

           services.AddHttpContextAccessor();
           services.AddScoped<AuthDelegatingHandler>();
        }

        public static IHttpClientBuilder RegisterAuthClient(this IHttpClientBuilder clientBuilder)
        {
            return clientBuilder.AddHttpMessageHandler<AuthDelegatingHandler>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
        }
    }
}
