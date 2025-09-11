namespace QueueMessageManagement.Config;

/// <summary>
/// RabbitMq Server and Queue options with default values assigned
/// </summary>
public class RabbitMqOptions
{
    public string HostName { get; set; } = DefaultOptions.DefaultHostName;
    
    public string VirtualHost { get; set; } = DefaultOptions.DefaultVirtualHost;
    
    public string UserName { get; set; } = DefaultOptions.DefaultUserName;
    
    public string Password { get; set; } = DefaultOptions.DefaultPassword;
    
    public int Port { get; set; } = DefaultOptions.DefaultPort;

    public QueueOptions DefaultQueue { get; set; } = new();

    public List<QueueOptions> Queues { get; set; } = [];
}