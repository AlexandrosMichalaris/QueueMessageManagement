namespace QueueMessageManagement.Interfaces;

public interface IProducer : IAsyncDisposable
{
    Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
}