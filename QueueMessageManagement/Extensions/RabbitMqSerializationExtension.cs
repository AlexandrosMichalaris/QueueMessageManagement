using System.Text.Json;

namespace QueueMessageManagement.Extentions;

public static class RabbitMqSerializationExtension
{
    public static byte[] ToRabbitMqMessage<T>(this T obj)
    {
        return JsonSerializer.SerializeToUtf8Bytes(obj);
    }
}