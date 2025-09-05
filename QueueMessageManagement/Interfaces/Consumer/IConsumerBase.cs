namespace QueueMessageManagement.Interfaces;

public interface IConsumerBase
{
    string QueueName { get; }
    
    Task ExecuteRawAsync(string jsonMessage, CancellationToken cancellationToken);
}