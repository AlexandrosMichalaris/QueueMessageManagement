using Microsoft.Extensions.Options;
using QueueMessageManagement.Config;
using QueueMessageManagement.Extentions;
using QueueMessageManagement.Interfaces;
using RabbitMQ.Client;

namespace QueueMessageManagement.Consumer;

public class QueueConfigurator : IQueueConfigurator
{
    private readonly RabbitMqOptions _options;

    public QueueConfigurator(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }
    
    public QueueOptions ResolveQueueOptions(string queueName) => 
        _options.QueueConfiguration.FirstOrDefault(q => q.QueueName == queueName)
        ?? _options.DefaultQueue.CloneWith(queueName);

    public async Task ConfigureQueueAsync(IChannel channel, QueueOptions queueOptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await channel.BasicQosAsync(0, queueOptions.PrefetchCount, false, cancellationToken);

        try
        {
            await channel.QueueDeclareAsync(
                queue: queueOptions.QueueName,
                durable: queueOptions.Durable, 
                exclusive: queueOptions.Exclusive,
                autoDelete: queueOptions.AutoDelete, 
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not declare queue '{queueOptions.QueueName}'", ex);
        }
    }
}