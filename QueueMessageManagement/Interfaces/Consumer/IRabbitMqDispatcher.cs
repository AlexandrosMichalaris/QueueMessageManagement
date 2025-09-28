namespace QueueMessageManagement.Interfaces;

public interface IRabbitMqDispatcher
{
    Task StartAllAsync(CancellationToken cancellationToken = default);
}