namespace QueueMessageManagement.Interfaces;

public interface IRabbitMqTopology
{
    Task EnsureTopologyAsync(CancellationToken cancellationToken);
}