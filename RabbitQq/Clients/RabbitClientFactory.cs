using Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitQq.Clients;

internal sealed class RabbitClientFactory
{
    private readonly RabbitContext _context;
    private readonly string _exchangeName;
    
    internal RabbitClientFactory(RabbitContext context, string exchangeName)
    {
        _context = context;
        _exchangeName = exchangeName;
    }

    internal RabbitReceiver CreateReceiver(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler)
        => new RabbitReceiver(_context, _exchangeName, options, handler);

    internal RabbitSender CreateSender(ExchangeDeclareOptions options)
        => new RabbitSender(_context, _exchangeName, options);
}