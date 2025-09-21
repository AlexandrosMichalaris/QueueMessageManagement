using System.Text.Json;
using QueueMessageManagement.Interfaces;
using QueueMessageManagement.Extentions;
using RabbitMQ.Client;

namespace QueueMessageManagement.Producer;

public class RabbitMqProducer : IProducer
{
    private readonly IRabbitMqConnection _connection;
    private IChannel? _channel;

    public RabbitMqProducer(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    private async Task<IChannel> GetOrCreateChannelAsync()
    {
        if (_channel != null && _channel.IsOpen)
            return _channel;

        _channel = await _connection.CreateChannelAsync();
        return _channel;
    }

    public async Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var channel = await GetOrCreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

        var props = new BasicProperties
        {
            Persistent = true,
            
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        
        //TODO: Add exchange 
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: props, body: body, cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
    }
}
