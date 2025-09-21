using Data_Center.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueueMessageManagement.Config;
using QueueMessageManagement.Consumer;
using QueueMessageManagement.Interfaces;
using QueueMessageManagement.Producer;

namespace QueueMessageManagement.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddQueueMessageManagement(this IServiceCollection services, IConfigurationSection configuration)
    {
        // Bind RabbitMqOptions from appsettings.json section
        services.Configure<RabbitMqOptions>(configuration);
        
        // Register hosted service to manage lifecycle
        services.AddHostedService<RabbitMqStartupService>();
        
        // Core Services
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddSingleton<IProducer, RabbitMqProducer>();
        services.AddSingleton<RabbitMqDispatcher>();

        return services;
    }
}