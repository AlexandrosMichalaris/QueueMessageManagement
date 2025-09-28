using Microsoft.Extensions.Hosting;
using QueueMessageManagement.Interfaces;
using QueueMessageManagement.Consumer;

namespace Data_Center.Configuration;

public class RabbitMqStartupService : IHostedService
{
    private readonly IRabbitMqConnection _connection;
    private readonly IRabbitMqDispatcher _dispatcher;
    private readonly IRabbitMqTopology _topology;

    public RabbitMqStartupService(IRabbitMqConnection connection, IRabbitMqDispatcher dispatcher, IRabbitMqTopology topology)
    {
        _connection = connection;
        _dispatcher = dispatcher;
        _topology = topology;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _connection.ConnectAsync(cancellationToken);
        await _dispatcher.StartAllAsync(cancellationToken);
        await _topology.EnsureTopologyAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // No StopAllAsync needed — DI will call DisposeAsync on both
        return Task.CompletedTask;
    }
}