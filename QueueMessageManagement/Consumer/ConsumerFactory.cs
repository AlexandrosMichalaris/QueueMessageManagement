using System.Text;
using System.Text.Json;
using QueueMessageManagement.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueMessageManagement.Consumer;

public class ConsumerFactory : IConsumerFactory
{
    public Task<AsyncEventingBasicConsumer> Create(IChannel channel, IConsumerBase consumer, ConsumerMetadata metadata, CancellationToken cancellationToken = default)
    {
        var asyncConsumer = new AsyncEventingBasicConsumer(channel);

        asyncConsumer.ReceivedAsync += async (sender, ea) =>
        {
            // ea is BasicDeliverEventArgs
            var bodyBytes = ea.Body.ToArray(); // ReadOnlyMemory<byte> -> byte[]
            var json = Encoding.UTF8.GetString(bodyBytes);

            try
            {
                var message = JsonSerializer.Deserialize(json, metadata.MessageType);

                // Dynamically call ExecuteAsync(message, cancellationToken)
                var task = (Task)metadata.ExecuteMethod.Invoke(consumer, new object[] { message!, cancellationToken })!;
                await task;

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message from {consumer.QueueName}: {ex.Message}");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false,
                    cancellationToken: cancellationToken);
            }
        };
        return Task.FromResult(asyncConsumer);
    }
}