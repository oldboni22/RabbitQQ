using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Options;
using RabbitMQ.Client.Events;
using RabbitQq.Abstractions;
using RabbitQq.Clients;

namespace RabbitQq;

internal class RabbitPipeline : IRabbitPipeline
{
    private readonly string _exchange;
    private readonly RabbitClientFactory _factory;
    private readonly RabbitContext _context;
    
    private readonly RabbitSender _sender;
    private readonly ConcurrentDictionary<string,RabbitReceiver> _dictionary = new();
    
    internal RabbitPipeline(RabbitContext context, string exchange, ExchangeDeclareOptions options)
    {
        _exchange = exchange;
        _context = context;
        _factory = new(context,_exchange);

        _sender = _factory.CreateSender(options);
    }

    internal async Task InitializeAsync()
    {
        await _sender.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await Parallel.ForEachAsync(_dictionary.Values, 
            async (receiver, token) =>
        {
            await receiver.DisposeAsync();
        });
    }

    #region Publisher
    public async Task BasicPublishAsync(string route, string body)
    {
        await _sender.BasicPublishAsync(route, body);
    }
    #endregion
    
    #region Receivers
    public async Task RegisterReceiverAsync(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler)
    {
        if (_dictionary.ContainsKey(options.Queue))
        {
            _context._logger?.LogWarning($"A receiver with the name {options.Queue} is already registered in the pipeline {_exchange}.");
            return;
        }
        
        var receiver = _factory.CreateReceiver(options,handler);
        await receiver.InitializeAsync();
        
        _dictionary.TryAdd(options.Queue, receiver);
    }

    public async Task DisposeReceiverAsync(string queue)
    {
        if (_dictionary.TryRemove(queue,out var value) is false)
        {
            _context._logger?.LogWarning($"A receiver with the name {queue} is not registered in the pipeline {_exchange}.");
            return;
        }
        
        await value.DisposeAsync();
    }
    #endregion
    
}