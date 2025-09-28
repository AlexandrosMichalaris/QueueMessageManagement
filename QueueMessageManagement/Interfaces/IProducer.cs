namespace QueueMessageManagement.Interfaces;

public interface IProducer : IAsyncDisposable
{
    Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);

    Task SendToDirectExchangeAsync<T>(string exchangeName, string routingKey, T message, CancellationToken cancellationToken = default);
    
    Task SendToFanoutExchangeAsync<T>(string exchangeName, T message, CancellationToken cancellationToken = default);
}