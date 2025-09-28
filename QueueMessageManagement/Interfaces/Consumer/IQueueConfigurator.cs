using QueueMessageManagement.Config;
using RabbitMQ.Client;

namespace QueueMessageManagement.Interfaces;

public interface IQueueConfigurator
{
    QueueOptions ResolveQueueOptions(string queueName);

    Task ConfigureQueueAsync(IChannel channel, QueueOptions queueOptions, CancellationToken cancellationToken);
}