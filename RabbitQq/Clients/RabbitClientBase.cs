using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitQq.Clients;

public class RabbitClientBase : IAsyncDisposable
{
    protected readonly RabbitContext _context;
    protected IChannel? _channel;
    
    protected RabbitClientBase(RabbitContext context)
    {
        _context = context;
    }
    
    protected async Task CheckConnectionAvailability()
    {
        if (_context.Connection == null || _context.Connection.IsOpen is false) 
        { 
            _context.Logger?.LogWarning("The connection with RabbitMq is unavailable. Trying to Reinitialize.");
            await _context.InitializeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}