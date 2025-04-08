using System.Reflection;
using AuthNuget.Registration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.OpenApi.Models;
using RabbitMqNuget.Registration;
using Time.Commands.Interfaces;
using Time.Commands.Seedwork;
using Time.Controllers;
using Time.Dispatchers;
using Time.Domain.DomainEvents;
using Time.HostedServices;
using Time.Middlewares;
using Time.Queries.Seedwork;
using Time.Repositories;

namespace Time;

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
        ScrutorScanForType(collection, typeof(IQueryHandler<,>), assemblyNames: "Time");
        ScrutorScanForType(collection, typeof(ICommandHandler<>), assemblyNames: "Time");
    }

    private void RegisterPresentation(IServiceCollection collection)
    {
        collection.AddHostedService<TickJob>();

        collection.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).PartManager.ApplicationParts.Add(new AssemblyPart(typeof(TimeController).Assembly));
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddSingleton(typeof(IInMemoryStore<>), typeof(InMemoryStore<>));

        collection.RegisterMassTransit(
            _configuration.GetConnectionString("Rabbitmq") ?? throw new InvalidOperationException("Rabbitmq connection string is not found"),
            new MassTransitConfigurator()
                .AddPublisher<DayStarted>("day-started-exchange"));

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