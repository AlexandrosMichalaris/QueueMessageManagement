using System.Text;
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
        var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, mandatory: false, basicProperties: this.GetBasicProperties(), body: body, cancellationToken: cancellationToken);
    }

    public async Task SendToDirectExchangeAsync<T>(string exchangeName, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, mandatory: false, basicProperties: this.GetBasicProperties(), body: body, cancellationToken: cancellationToken);
    }

    public async Task SendToFanoutExchangeAsync<T>(string exchangeName, T message, CancellationToken cancellationToken = default)
    {
        var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // routingKey is ignored in fanout exchange
        await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, mandatory: false, basicProperties: this.GetBasicProperties(), body: body, cancellationToken: cancellationToken);
    }

    private BasicProperties GetBasicProperties() => new BasicProperties() { Persistent = true };

    public async ValueTask DisposeAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
    }
}
