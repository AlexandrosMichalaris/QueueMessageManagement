namespace QueueMessageManagement.Config;

public class ExchangeOptions
{
    public string Name { get; init; } = string.Empty;
    
    public string Type { get; init; } = DefaultOptions.DefaultExchangeType;

    public bool Durable { get; init; } = true;
    
    public bool AutoDelete { get; init; } = false;
    
    public IEnumerable<Bindings> Bindings { get; init; } = Array.Empty<Bindings>();
}