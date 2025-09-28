namespace QueueMessageManagement.Config;

/// <summary>
/// Default Values used int the configuration options of queues and RabbitMq Server
/// </summary>
public static class DefaultOptions
{
    // RabbitMq Default options for server
    public const string DefaultHostName = "localhost";
    
    public const string DefaultVirtualHost = "/";
    
    public const string DefaultUserName = "guest";
    
    public const string DefaultPassword = "guest";
    
    public const int DefaultPort = 5672;
    
    // Queue message options
    public const int DefaultRetryCount = 3;
    
    public const string DefaultExchangeType = "direct";
}