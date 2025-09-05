namespace QueueMessageManagement.Interfaces;

public interface IConsumerWithMetadata<T> : IConsumer<T> where T : class
{
    string ExchangeName { get; }
    
    string RoutingKey { get; }
    
    bool Durable { get; }
}