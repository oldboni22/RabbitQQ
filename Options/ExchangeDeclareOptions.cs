namespace Options;

public record ExchangeDeclareOptions
    (
        ExchangeTypeEnum TypeEnum,
        bool Durable,
        bool Exclusive,
        bool AutoDelete,
        bool Passive,
        bool NoWait
    );


public enum ExchangeTypeEnum
{
    Direct,
    Topic
}