namespace QueueMessageManagement.Config;

/// <summary>
/// RabbitMq Server and Queue options with default values assigned
/// </summary>
public class RabbitMqOptions
{
    public string HostName { get; init; } = DefaultOptions.DefaultHostName;
    
    public string VirtualHost { get; init; } = DefaultOptions.DefaultVirtualHost;
    
    public string UserName { get; init; } = DefaultOptions.DefaultUserName;
    
    public string Password { get; init; } = DefaultOptions.DefaultPassword;
    
    public int Port { get; init; } = DefaultOptions.DefaultPort;

    public QueueOptions DefaultQueue { get; init; } = new();
    
    public IEnumerable<ExchangeOptions> Exchanges { get; init; } = Array.Empty<ExchangeOptions>();
    
    public IEnumerable<QueueOptions> QueueConfiguration { get; init; } = Array.Empty<QueueOptions>();
}