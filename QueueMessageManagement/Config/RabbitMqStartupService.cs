using Microsoft.Extensions.Hosting;
using QueueMessageManagement.Interfaces;
using QueueMessageManagement.Consumer;

namespace Data_Center.Configuration;

public class RabbitMqStartupService : IHostedService
{
    private readonly IRabbitMqConnection _connection;
    private readonly RabbitMqDispatcher _dispatcher;

    public RabbitMqStartupService(IRabbitMqConnection connection, RabbitMqDispatcher dispatcher)
    {
        _connection = connection;
        _dispatcher = dispatcher;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _connection.ConnectAsync();
        await _dispatcher.StartAllAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // No StopAllAsync needed — DI will call DisposeAsync on both
        return Task.CompletedTask;
    }
}