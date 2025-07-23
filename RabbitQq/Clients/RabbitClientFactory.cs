using Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitQq.Clients;

internal sealed class RabbitClientFactory
{
    private readonly RabbitContext _context;
    private readonly string _exchange;
    
    internal RabbitClientFactory(RabbitContext context, string exchange)
    {
        _context = context;
        _exchange = exchange;
    }

    internal RabbitReceiver CreateReceiver(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler)
        => new RabbitReceiver(_context, options, _exchange, handler);

}