using QueueMessageManagement.Config;

namespace QueueMessageManagement.Extentions;

public static class QueueOptionsExtension
{
    public static QueueOptions CloneWith(this QueueOptions source, string queueName)
    {
        return new QueueOptions()
        {
            QueueName = queueName,
            AutoDelete = source.AutoDelete,
            Durable = source.Durable,
            Exclusive = source.Exclusive,
            PrefetchCount = source.PrefetchCount,
            RetryCount = source.RetryCount
        };
    }
}