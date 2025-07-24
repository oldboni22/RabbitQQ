namespace Options;

public record ExchangeDeclareOptions
    (
        ExchangeTypeEnum TypeEnum,
        bool Durable,
        bool Exclusive,
        bool AutoDelete,
        bool Passive,
        bool NoWait,
        string RoutingKey,
        bool AutoAck
    );


public enum ExchangeTypeEnum
{
    Direct,
    Topic
}