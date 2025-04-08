using System.Reflection;
using AuthNuget.Registration;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using News.Commands.Interfaces;
using News.Commands.Seedwork;
using News.Consumers;
using News.Controllers;
using News.Dispatchers;
using News.Interfaces;
using News.Middlewares;
using News.Queries.Seedwork;
using News.Repositories;
using RabbitMqNuget.Registration;

namespace News;

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
            endpoints.MapSwagger();
        });
    }

    private void RegisterApplication(IServiceCollection collection)
    {
        ScrutorScanForType(collection, typeof(IQueryHandler<,>), assemblyNames: "News");
        ScrutorScanForType(collection, typeof(ICommandHandler<>), assemblyNames: "News");
    }

    private void RegisterPresentation(IServiceCollection collection)
    {
        collection.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).PartManager.ApplicationParts.Add(new AssemblyPart(typeof(NewsController).Assembly));
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddSingleton<IMongoClient>(_ => new MongoClient(_configuration.GetConnectionString("Mongodb")));

        collection.AddScoped<IArticleRepository, MongoArticleRepository>();

        collection.AddSingleton<IAzureBlobRepository, AzureBlobRepository>(_
            => new AzureBlobRepository(new BlobContainerClient(_configuration.GetConnectionString("Blob") ?? throw new InvalidOperationException("Blob connection string is not found"),
                "article-contents")));

        collection.RegisterMassTransit(
            _configuration.GetConnectionString("Rabbitmq") ??
            throw new InvalidOperationException("Rabbitmq connection string is not found"),
            new MassTransitConfigurator()
                .AddConsumer<News.Consumers.Messages.News, NewsConsumer>("news-exchange", sp =>
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