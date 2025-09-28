using QueueMessageManagement.Consumer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueMessageManagement.Interfaces;

public interface IConsumerFactory
{
    Task<AsyncEventingBasicConsumer> Create(IChannel channel, IConsumerBase consumer, ConsumerMetadata metadata,
        CancellationToken cancellationToken);
}