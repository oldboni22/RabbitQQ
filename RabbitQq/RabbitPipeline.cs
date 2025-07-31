using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Options;
using RabbitMQ.Client.Events;
using RabbitQq.Abstractions;
using RabbitQq.Clients;

namespace RabbitQq;

internal class RabbitPipeline : IRabbitPipeline
{
    private readonly string _exchangeName;
    private readonly RabbitClientFactory _clientFactory;
    private readonly RabbitContext _context;
    
    private readonly RabbitSender _sender;
    private readonly ConcurrentDictionary<string,RabbitReceiver> _dictionary = new();
    
    internal RabbitPipeline(RabbitContext context, string exchangeName, ExchangeDeclareOptions options)
    {
        _exchangeName = exchangeName;
        _context = context;
        _clientFactory = new(context,_exchangeName);

        _sender = _clientFactory.CreateSender(options);
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
            _context._logger?.LogWarning($"A receiver with the name {options.Queue} is already registered in the pipeline {_exchangeName}.");
            return;
        }
        
        var receiver = _clientFactory.CreateReceiver(options,handler);
        await receiver.InitializeAsync();
        
        _dictionary.TryAdd(options.Queue, receiver);
    }

    public async Task DisposeReceiverAsync(string queueName)
    {
        if (_dictionary.TryRemove(queueName,out var value) is false)
        {
            _context._logger?.LogWarning($"A receiver with the name {queueName} is not registered in the pipeline {_exchangeName}.");
            return;
        }
        
        await value.DisposeAsync();
    }
    #endregion
    
}