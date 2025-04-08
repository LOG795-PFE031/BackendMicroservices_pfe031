using System.Reflection;
using AuthNuget.Registration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Portfolio.Commands.Interfaces;
using Portfolio.Commands.Seedwork;
using Portfolio.Consumers;
using Portfolio.Consumers.Messages;
using Portfolio.Controllers;
using Portfolio.Dispatchers;
using Portfolio.Middlewares;
using Portfolio.Proxies;
using Portfolio.Proxies.Impl;
using Portfolio.Queries.Interfaces;
using Portfolio.Queries.Seedwork;
using Portfolio.Repositories;
using RabbitMqNuget.Registration;
using RabbitMqNuget.Services;
using RabbitMqNuget.Services.Impl;

namespace Portfolio;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        RegisterConfiguration(services);
        RegisterInfrastructure(services);
        RegisterPresentation(services);
        RegisterApplication(services);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        }
                    },
                    new string[] {}
                }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ApiLoggingMiddleware>();
        app.UseMiddleware<TransactionMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void RegisterApplication(IServiceCollection collection)
    {
        ScrutorScanForType(collection, typeof(IQueryHandler<,>), assemblyNames: "Portfolio");
        ScrutorScanForType(collection, typeof(ICommandHandler<>), assemblyNames: "Portfolio");
    }

    private void RegisterPresentation(IServiceCollection collection)
    {
        collection.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).PartManager.ApplicationParts.Add(new AssemblyPart(typeof(PortfolioController).Assembly));
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddHttpClient<AuthProxy>().RegisterAuthClient();
        collection.AddHttpClient<StockProxy>().RegisterAuthClient();
        collection.AddHttpClient<TimeProxy>().RegisterAuthClient();

        collection.AddScoped<IAuthProxy, AuthProxy>();
        collection.AddScoped<IStockProxy, StockProxy>();
        collection.AddScoped<ITimeProxy, TimeProxy>();

        collection.AddScoped<IMigrateWalletContext, WalletContext>(provider => provider.GetRequiredService<WalletContext>());

        collection.AddScoped<ITransactionInfo, TransactionInfo>();

        collection.AddDbContext<WalletContext>(RepositoryDbContextOptionConfiguration);

        collection.AddScoped<IWalletQueryContext, WalletRepository>(provider => provider.GetRequiredService<WalletRepository>());
        collection.AddScoped<IWalletRepository, WalletRepository>(provider => provider.GetRequiredService<WalletRepository>());
        collection.AddScoped<WalletRepository>();

        collection.RegisterMassTransit(
            _configuration.GetConnectionString("Rabbitmq") ?? throw new InvalidOperationException("Rabbitmq connection string is not found"),
            new MassTransitConfigurator()
                .AddPublisher<UserCreated>("user-created-exchange")
                .AddConsumer<UserCreated, UserCreatedConsumer>("user-created-exchange", sp => 
                {
                    var scope = sp.CreateScope();
                    return new(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>());
                }));

        collection.RegisterPfeAuthorization();

        return;

        void RepositoryDbContextOptionConfiguration(DbContextOptionsBuilder options)
        {
            var connectionString = _configuration.GetConnectionString("Postgres");

            options.EnableThreadSafetyChecks();
            options.UseNpgsql(connectionString);
        }
    }

    private void RegisterConfiguration(IServiceCollection collection)
    {
        collection.AddScoped<IQueryDispatcher, QueryDispatcher>();
        collection.AddScoped<ICommandDispatcher, CommandDispatcher>();
    }

    private void ScrutorScanForType(IServiceCollection services, Type type,
        ServiceLifetime lifetime = ServiceLifetime.Scoped, params string[] assemblyNames)
    {
        services.Scan(selector =>
        {
            selector.FromAssemblies(assemblyNames.Select(Assembly.Load))
                .AddClasses(filter => filter.AssignableTo(type))
                .AsImplementedInterfaces()
                .WithLifetime(lifetime);
        });
    }
}