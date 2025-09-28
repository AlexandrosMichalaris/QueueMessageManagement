namespace QueueMessageManagement.Config;

/// <summary>
/// Queue options with default values assigned
/// </summary>
public class QueueOptions
{
    public string QueueName { get; set; } = string.Empty;

    public bool AutoDelete { get; set; } = false;
    
    public bool Durable { get; set; } = true;
    
    public bool Exclusive { get; set; } = false;
    
    public ushort PrefetchCount { get; set; } = 1;
    
    public int RetryCount { get; set; } = DefaultOptions.DefaultRetryCount;
}