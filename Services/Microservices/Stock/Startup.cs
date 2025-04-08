using System.Reflection;
using AuthNuget.Registration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RabbitMqNuget.Registration;
using RabbitMqNuget.Services;
using RabbitMqNuget.Services.Impl;
using Stock.Commands.Interfaces;
using Stock.Commands.Seedwork;
using Stock.Consumers;
using Stock.Consumers.Messages;
using Stock.Controllers;
using Stock.Dispatchers;
using Stock.Queries.Seedwork;
using Stock.Repositories;

namespace Stock;

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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void RegisterApplication(IServiceCollection collection)
    {
        ScrutorScanForType(collection, typeof(IQueryHandler<,>), assemblyNames: "Stock");
        ScrutorScanForType(collection, typeof(ICommandHandler<>), assemblyNames: "Stock");
    }

    private void RegisterPresentation(IServiceCollection collection)
    {
        collection.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).PartManager.ApplicationParts.Add(new AssemblyPart(typeof(StockController).Assembly));
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddScoped<ITransactionInfo, TransactionInfo>();

        collection.AddSingleton<IMongoClient>(_ => new MongoClient(_configuration.GetConnectionString("Mongodb")));

        collection.AddScoped<ISharesRepository, MongoSharesRepository>();

        collection.RegisterMassTransit(
            _configuration.GetConnectionString("Rabbitmq") ??
            throw new InvalidOperationException("Rabbitmq connection string is not found"),
            new MassTransitConfigurator()
                .AddConsumer<StockQuote, QuoteConsumer>("quote-exchange", sp =>
                {
                    var scope = sp.CreateScope();
                    return new(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>());
                }));

        collection.RegisterPfeAuthorization();
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