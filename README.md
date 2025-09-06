# Queue Message Management

This library provides a clean **abstraction layer** for working with RabbitMQ producers, consumers and consumer dispatchers in .NET
It handles:
- RabbitMq Connection
- Producer interface for sending messages to specific queues
- Consumer interfaces for receiving messages
- Dispatcher that automatically wires up consumers

---

## Installation
Add the NuGet package reference
```
dotnet add package QueueMessageManagement
```

## Setup package in your project

1. Add RabbitMq configuration in appsettings.json
```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

2. Register library in program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Queue Management with RabbitMQ options from configuration
builder.Services.AddQueueMessageManagement(builder.Configuration);

var app = builder.Build();
app.Run();
```

And you are ready to go!

## Producer
To send a message to a desired queue, the IProducer interface is already injected and ready to be used.
```csharp
public class FileUploadService
{
    private readonly IProducer _producer;

    public FileUploadService(IProducer producer)
    {
        _producer = producer;
    }

    public async Task SendMessageAsync()
    {
        var message = new MyCustomMessage
        {
            Id = Guid.NewGuid(),
            Text = "Hello from Producer!"
        };

        await _producer.SendAsync<MyCustomMessage>("my-queue", message);
    }
}
```

##Consumer
To consume a message, inherit from the ConsumerBase<T> generic class and override the QueueName property and ExecuteAsync method.
```csharp
public class TestConsumer : ConsumerBase<MyCustomMessage>
{
    public override QueueName => "my-queue";

    public override async Task ExecuteAsync(MyCustomMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Consumer] Received: {message.Text}");
        await Task.CompletedTask;
    }
}
```

It is important to register your consumer class into the Dependency Injection framework as a **Singleton**
```csharp
services.AddSingleton<IConsumerBase, TestConsumer>();
```

If you register your consumers as **Scoped**, you will get a:
```csharp
InvalidOperationException: Cannot consume scoped service ‘X’ from singleton ‘Y’.
```
because the dispatcher is registered as a singleton service.

## Chaining Consumers

If you want one consumer to trigger another queue (pipeline processing):
```csharp
public class ProcessedFileConsumer : IConsumer<FileProcessedEvent>
{
    private readonly IProducer _producer;

    public ProcessedFileConsumer(IProducer producer)
    {
        _producer = producer;
    }

    public string QueueName => "file-processed";

    public async Task ExecuteAsync(FileProcessedEvent message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"File processed: {message.FileId}");

        // Chain → publish next event
        var nextMessage = new ArchiveFileEvent { FileId = message.FileId };
        await _producer.SendAsync("archive-queue", nextMessage, cancellationToken);
    }
}
```

## Things to configure before using the NuGet
1.	Add RabbitMq section in appsettings.json (connection string and credentials).
2.	Register all consumers in DI using services.AddSingleton<IConsumer<T>, MyConsumer>().
3. Always start the dispatcher (RabbitMqDispatcher) at application startup (this is handled automatically by the NuGet’s hosted service).

