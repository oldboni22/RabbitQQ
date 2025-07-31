using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitQq.Clients;

internal class RabbitClientBase : IAsyncDisposable
{
    private bool _disposed = false;
    
    protected readonly RabbitContext _context;
    protected IChannel? _channel;
    protected readonly string _exchangeName;
    
    protected RabbitClientBase(RabbitContext context, string exchangeName)
    {
        _context = context;
        _exchangeName = exchangeName;
    }

    protected async Task CheckConnectionAvailability()
    {
        if (_context.Connection == null || _context.Connection.IsOpen is false) 
        { 
            _context._logger?.LogWarning("The connection with RabbitMq is unavailable. Trying to Reinitialize.");
            await _context.InitializeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if(_disposed)
            return;

        _disposed = true;
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}