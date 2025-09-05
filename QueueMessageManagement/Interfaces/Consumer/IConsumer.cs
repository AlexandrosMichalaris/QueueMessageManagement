namespace QueueMessageManagement.Interfaces;

public interface IConsumer<T> : IConsumerBase where T : class
{
    Task ExecuteAsync(T message, CancellationToken cancellationToken);
}