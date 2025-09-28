using System.Text.Json;
using QueueMessageManagement.Interfaces;

namespace QueueMessageManagement.Consumer;

/// <summary>
/// This way we don’t repeat serialization logic in every consumer
/// So we implement ExecuteRawAsync
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ConsumerBase<T> : IConsumer<T> where T : class
{
    public abstract string QueueName { get; }

    public abstract Task ExecuteAsync(T message, CancellationToken cancellationToken);

    public async Task ExecuteRawAsync(string json, CancellationToken cancellationToken)
    {
        var obj = JsonSerializer.Deserialize<T>(json);
        await ExecuteAsync(obj!, cancellationToken);
    }
}