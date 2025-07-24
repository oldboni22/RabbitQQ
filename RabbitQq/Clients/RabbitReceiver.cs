using Microsoft.Extensions.Logging;
using Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitQq.Clients;

internal sealed class RabbitReceiver : RabbitClientBase
{
    private readonly ReceiverQueueOptions _options;
    private readonly AsyncEventHandler<BasicDeliverEventArgs> _handler;
    
    internal RabbitReceiver(RabbitContext context, string exchange,
        ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler) : 
        base(context,exchange)
    {
        _options = options;
        _handler = handler;
    }

    internal async Task InitializeAsync()
    {
        await CheckConnectionAvailability();

        try
        {
            _channel = await _context.Connection!.CreateChannelAsync();

            await _channel.QueueDeclareAsync
            (
                queue: _options.Queue,
                durable: _options.Durable,
                exclusive: _options.Exclusive,
                autoDelete: _options.AutoDelete,
                passive: _options.Passive,
                noWait: _options.NoWait
            );

            await _channel.QueueBindAsync
            (
                queue: _options.Queue,
                exchange: _exchange,
                routingKey: _options.RoutingKey,
                noWait: _options.NoWait
            );


            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += _handler;

            await _channel.BasicConsumeAsync
            (
                queue: _options.Queue,
                consumer: consumer,
                autoAck: _options.AutoAck
            );
        }
        catch(Exception ex)
        {
            _context._logger?.LogError(ex, $"An exception occured while initializing a receiver in exchange {_exchange}.");
            throw;
        }
    }
    
}