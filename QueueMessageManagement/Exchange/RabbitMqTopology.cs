using Microsoft.Extensions.Options;
using QueueMessageManagement.Config;
using QueueMessageManagement.Interfaces;

namespace QueueMessageManagement.Topology;

public class RabbitMqTopology : IRabbitMqTopology
{
    private readonly IRabbitMqConnection _connection;
    private readonly RabbitMqOptions _options;

    public RabbitMqTopology(IRabbitMqConnection connection, IOptions<RabbitMqOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }
    
    public async Task EnsureTopologyAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Exchanges.Any())
            return;
        
        var channel = await _connection.CreateChannelAsync();

        foreach (var exchange in _options.Exchanges)
        {
            try
            {
                // Declare exchange
                await channel.ExchangeDeclareAsync(
                    exchange: exchange.Name,
                    type: exchange.Type,
                    durable: exchange.Durable,
                    autoDelete: exchange.AutoDelete,
                    cancellationToken: cancellationToken);

                // Declare bindings
                foreach (var binding in exchange.Bindings)
                {
                    await channel.QueueBindAsync(
                        queue: binding.Queue,
                        exchange: exchange.Name,
                        routingKey: binding.RoutingKey,
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not declare exchange '{exchange.Name}'", ex);
            }
        }
    }
}