using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitQq.Abstractions;

namespace RabbitQq;

internal sealed class RabbitContext : IRabbitContext
{
    internal ConcurrentDictionary<string, RabbitPipeline> Dictionary { get; set; }
    private readonly ConnectionFactory _connectionFactory;

    internal readonly ILogger? _logger;
    internal IConnection? Connection { get; private set; }
    
    internal RabbitContext(ConnectionFactory connectionFactory, ILogger? logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    
    public async ValueTask DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
        }

        await Parallel.ForEachAsync(Dictionary, async (pair,_) =>
        {
            await pair.Value.DisposeAsync();
        });
    }
    
    internal async Task InitializeAsync()
    {
        try
        {
            Connection = await _connectionFactory.CreateConnectionAsync();
            _logger?.LogInformation("RabbitMq connection created successfully.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An exception occured while opening connection.");
            throw;
        }
    }

   
}