namespace QueueMessageManagement.Config;

public class QueueSettings
{
    public string HostName { get; set; }
    
    public string Exchange { get; set; }
    
    public List<string> Queues { get; set; } = new();
}