using System.Text;
using System.Text.Json;
using QueueMessageManagement.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QueueMessageManagement.Consumer;

public class RabbitMqDispatcher
{
    private readonly IRabbitMqConnection _connection;
    private readonly IEnumerable<IConsumerBase> _consumers;

    public RabbitMqDispatcher(IRabbitMqConnection connection, IEnumerable<IConsumerBase> consumers)
    {
        _connection = connection;
        _consumers = consumers;
    }

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var consumerObj in _consumers)
        {
            // Find IConsumer<T> implemented by this object
            var consumerInterface = consumerObj.GetType()
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConsumer<>));

            if (consumerInterface == null)
                continue; // Not a valid consumer

            var messageType = consumerInterface.GetGenericArguments()[0];
            var queueName = (string)consumerObj
                .GetType()
                .GetProperty(nameof(IConsumer<object>.QueueName))!
                .GetValue(consumerObj)!;

            // Create channel & ensure queue exists
            var channel = await _connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            var asyncConsumer = new AsyncEventingBasicConsumer(channel);

            asyncConsumer.ReceivedAsync += async (sender, ea) =>
            {
                // ea is BasicDeliverEventArgs
                var bodyBytes = ea.Body.ToArray(); // ReadOnlyMemory<byte> -> byte[]
                var json = Encoding.UTF8.GetString(bodyBytes);

                try
                {
                    var message = JsonSerializer.Deserialize(json, messageType);

                    // Dynamically call ExecuteAsync(message, cancellationToken)
                    var executeMethod = consumerInterface.GetMethod(nameof(IConsumer<object>.ExecuteAsync))!;
                    var task = (Task)executeMethod.Invoke(consumerObj, new object[] { message!, cancellationToken })!;
                    await task;

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message from {queueName}: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: asyncConsumer, cancellationToken: cancellationToken);
        }
    }
}