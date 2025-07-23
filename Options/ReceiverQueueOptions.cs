namespace Options;

public record ReceiverQueueOptions
    (
        string Queue,
        bool Durable,
        bool Exclusive,
        bool AutoDelete,
        bool Passive,
        bool NoWait,
        string RoutingKey,
        bool AutoAck
    );