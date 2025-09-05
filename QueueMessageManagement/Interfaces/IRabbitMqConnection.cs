using RabbitMQ.Client;

namespace QueueMessageManagement.Interfaces;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task ConnectAsync();
    
    Task<IChannel> CreateChannelAsync();
}