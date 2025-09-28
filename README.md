# Queue Message Management

This library provides a clean **abstraction layer** for working with RabbitMQ producers, consumers and consumer dispatchers in .NET
It handles:
- RabbitMq Connection
- Producer interface for sending messages to specific queues
- Consumer interfaces for receiving messages
- Dispatcher that automatically wires up consumers
- Configure exchanges and binding that will be automatically mapped on start up.

---

## Installation
Add the NuGet package reference
```
dotnet add package QueueMessageManagement
```

## Setup package in your project

1. **Add RabbitMq configuration in appsettings.json**

Starting from version 2.0.0, you can configure:
- RabbitMQ connection (hostname, vhost, credentials)
- Default queue settings (used if not explicitly overridden)
- Per-queue settings
- Exchanges and bindings

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchanges": [
      {
        "Name": "files",
        "Type": "direct",
        "Durable": true,
        "AutoDelete": false,
        "Bindings": [
          { "Queue": "file-chunks", "RoutingKey": "upload" },
          { "Queue": "file-reassembly", "RoutingKey": "event" }
        ]
      }
    ],
    "DefaultQueue": {
      "Durable": true,
      "PrefetchCount": 5,
      "RetryCount": 3
    },
    "QueueConfiguration": [
      {
        "QueueName": "file-chunks",
        "Durable": true,
        "PrefetchCount": 10
      },
      {
        "QueueName": "file-reassembly",
        "Durable": false,
        "PrefetchCount": 1
      }
    ]
  }
}
```

Note: 
- If no configuration is provided, default values will automatically be used for both the RabbitMQ connection and the queues.
- If you declare an exchange that already exists, it must have the same type & durability as before, otherwise RabbitMQ will throw an error.
- Queues are created and registered only when a corresponding consumer is implemented and actively listening to that specific queue. If it only exists in the appsettings.json, **it won't be created automatically**.

2. Register library in program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Queue Management with RabbitMQ options from configuration
builder.Services.AddQueueMessageManagement(builder.Configuration);

var app = builder.Build();
app.Run();
```

And you are ready to go!

---

## Producer
To send a message to a desired queue, the IProducer interface is already injected and ready to be used.

### Send message directly to a queue

```csharp
await _producer.SendAsync<MyCustomMessage>("my-queue", message, cancellationToken);
```
The `SendAsync` method raises a message directly to the queue via the default and empty exchange.

### Send message to a direct exchange with a routing key

```csharp
await _producer.SendToDirectExchangeAsync<MyCustomMessage>(
    exchangeName: "files",
    routingKey: "upload",
    message: message,
    cancellationToken: cancellationToken);
```
- The message will be routed only to queues bound to the exchange with the matching routing key.


### Send to a fanout exchange

```csharp
await _producer.SendToFanoutExchangeAsync<MyCustomMessage>(
    exchangeName: "broadcast-files",
    message: message,
    cancellationToken: cancellationToken);
```
The message will be delivered to **all queues bound** to the fanout exchange, regardless of routing key.

---

## Consumer
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

### Consumer Registration

It is important to register your consumer class into the Dependency Injection framework as a **Singleton**
```csharp
services.AddSingleton<IConsumerBase, TestConsumer>();
```

If you register your consumers as **Scoped**, you will get a:
```csharp
InvalidOperationException: Cannot consume scoped service ‘X’ from singleton ‘Y’.
```
because the dispatcher is registered as a singleton service.

### Queue Registration

A queue is **only declared if there is a consumer that implements IConsumer<T> for it**.
Example: If you define a queue in appsettings.json but never implement a consumer for it, the queue will not be created.
This ensures that the topology reflects the actual application behavior: only queues with consumers are created and listened to.

---

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

---

## Things to configure before using the NuGet
1.	Add RabbitMq section in appsettings.json (connection string and credentials).
2.	Register all consumers in DI using services.AddSingleton<IConsumer<T>, MyConsumer>().
3. Always start the dispatcher (RabbitMqDispatcher) at application startup (this is handled automatically by the NuGet’s hosted service).

---

## Important Considerations.

### **PRECONDITION_FAILED (406)**

Occurs if you try to declare an exchange that already exists with different type or durability. 
Ensure exchanges are declared consistently across all applications. Do not try to change an existing exchange’s type or durability dynamically.

### **Mismatched Routing Key**

Sending to a direct exchange with a routing key that has no matching bound queue will result in the message being dropped (unless mandatory flag is used). 

### **Bindings require queues to exist first**

The dispatcher ensures queues are declared before bindings are created. If the queue is missing, the binding will fail.

### **Multiple Applications**

Declaring the same exchange/queue in different apps is fine as long as the settings match. If not, one of them will crash on startup.