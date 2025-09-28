namespace QueueMessageManagement.Config;

public class Bindings
{
    public string Queue { get; init; } = null!;

    public string RoutingKey { get; init; } = null!;
}