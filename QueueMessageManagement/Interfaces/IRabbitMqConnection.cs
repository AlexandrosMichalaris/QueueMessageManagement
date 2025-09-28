using RabbitMQ.Client;

namespace QueueMessageManagement.Interfaces;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    Task<IChannel> CreateChannelAsync();
}