using System.Collections.Concurrent;
using Options;
using RabbitMQ.Client.Events;
using RabbitQq.Abstractions;
using RabbitQq.Clients;

namespace RabbitQq;

internal class RabbitPipeline : IRabbitPipeline
{
    private readonly string _exchange;
    private readonly RabbitClientFactory _factory;
    
    private readonly ConcurrentBag<RabbitReceiver> _receivers = new();
    
    internal RabbitPipeline(RabbitContext context, string exchange)
    {
        _exchange = exchange;
        _factory = new(context,_exchange);
    }

    public async ValueTask DisposeAsync()
    {
        await Parallel.ForEachAsync(_receivers, async (receiver, token) =>
        {
            await receiver.DisposeAsync();
        });
    }
    
    public async Task RegisterReceiverAsync(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler)
    {
        var receiver = _factory.CreateReceiver(options,handler);
        await receiver.InitializeAsync();
        
        _receivers.Add(receiver);
    }
    
}