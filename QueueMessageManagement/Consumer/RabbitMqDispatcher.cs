using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QueueMessageManagement.Config;
using QueueMessageManagement.Extentions;
using QueueMessageManagement.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueMessageManagement.Consumer;

public class RabbitMqDispatcher : IRabbitMqDispatcher
{
    private readonly IRabbitMqConnection _connection;
    private readonly IEnumerable<IConsumerBase> _consumers;
    private readonly RabbitMqOptions _options;
    private readonly IQueueConfigurator _queueConfigurator;
    private readonly IConsumerFactory _consumerFactory;

    #region Ctor

    public RabbitMqDispatcher(
        IRabbitMqConnection connection, 
        IEnumerable<IConsumerBase> consumers,
        IOptions<RabbitMqOptions> options,
        IQueueConfigurator queueConfigurator,
        IConsumerFactory consumerFactory)
    {
        _connection = connection;
        _consumers = consumers;
        _options = options.Value;
        _queueConfigurator = queueConfigurator;
        _consumerFactory = consumerFactory;
    }

    #endregion

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        foreach (var consumer in _consumers)
        {
            var metadata = ConsumerMetadata.From(consumer);
            if (metadata == null) continue;

            var channel = await _connection.CreateChannelAsync();

            try
            {
                var queueOptions = _queueConfigurator.ResolveQueueOptions(metadata.QueueName);
                await _queueConfigurator.ConfigureQueueAsync(channel, queueOptions, cancellationToken);

                var asyncConsumer = await _consumerFactory.Create(channel, consumer, metadata, cancellationToken);
                await channel.BasicConsumeAsync(queueOptions.QueueName, false, asyncConsumer, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error initializing consumer for queue '{metadata.QueueName}'", ex);
            }
        }
    }
}