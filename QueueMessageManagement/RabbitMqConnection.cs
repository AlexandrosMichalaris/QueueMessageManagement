using Microsoft.Extensions.Options;
using QueueMessageManagement.Config;
using QueueMessageManagement.Interfaces;
using RabbitMQ.Client;

namespace QueueMessageManagement;

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options)
    {
        var config = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = config.HostName,
            Port = config.Port,
            VirtualHost = config.VirtualHost,
            UserName = config.UserName,
            Password = config.Password,
        };
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is null || !_connection.IsOpen)
            throw new InvalidOperationException("RabbitMQ connection is not open.");

        return await _connection.CreateChannelAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}