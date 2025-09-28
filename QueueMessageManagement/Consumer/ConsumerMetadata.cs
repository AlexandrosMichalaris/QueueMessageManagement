using System.Reflection;
using Microsoft.Extensions.Options;
using QueueMessageManagement.Config;
using QueueMessageManagement.Extentions;
using QueueMessageManagement.Interfaces;

namespace QueueMessageManagement.Consumer;

public record ConsumerMetadata(
    Type MessageType,
    MethodInfo ExecuteMethod,
    string QueueName)
{
    public static ConsumerMetadata? From(IConsumerBase consumer)
    {
        var consumerInterface = consumer.GetType()
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IConsumer<>));

        if (consumerInterface == null) return null;

        var messageType = consumerInterface.GetGenericArguments()[0];
        var executeMethod = consumerInterface.GetMethod(nameof(IConsumer<object>.ExecuteAsync))!;
        var queueName = (string)consumer.GetType()
            .GetProperty(nameof(IConsumer<object>.QueueName))!
            .GetValue(consumer)!;

        return new ConsumerMetadata(messageType, executeMethod, queueName);
    }
}