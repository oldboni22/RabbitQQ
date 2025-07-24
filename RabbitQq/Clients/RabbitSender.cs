using System.Text;
using Microsoft.Extensions.Logging;
using Options;
using RabbitMQ.Client;

namespace RabbitQq.Clients;

internal sealed class RabbitSender : RabbitClientBase
{
    private readonly ExchangeDeclareOptions _options;
    
    public RabbitSender(RabbitContext context, string exchange, ExchangeDeclareOptions options) : base(context, exchange)
    {
        _options = options;
    }
    
    private string ExchangeTypeSwitch(ExchangeTypeEnum typeEnum)
    {
        switch (typeEnum)
        {
            case ExchangeTypeEnum.Direct:
            {
                return ExchangeType.Direct;
            }
            case ExchangeTypeEnum.Topic:
            {
                return ExchangeType.Topic;
            }
        }
        
        return ExchangeType.Direct;
    }
    
    public async Task InitializeAsync()
    {
        await CheckConnectionAvailability();

        try
        {
            _channel = await _context.Connection!.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync
            (
                exchange: _exchange,
                type: ExchangeTypeSwitch(_options.TypeEnum),
                durable: _options.Durable,
                autoDelete: _options.AutoDelete,
                passive: _options.Passive,
                noWait: _options.NoWait
            );
        }
        catch (Exception ex)
        {
            _context._logger?.LogError(ex, $"An exception occured while initializing a sender in exchange {_exchange}.");
            throw;
        }
    }

    public async Task BasicPublishAsync(string route, string body)
    {
        await CheckConnectionAvailability();

        try
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            await _channel!.BasicPublishAsync
            (
                _exchange,
                routingKey: route,
                body: bytes
            );
        }
        catch (Exception ex)
        {
            _context._logger?.LogError(ex, $"An exception occured while publishing in exchange {_exchange}.");
            throw;
        }
    }
}