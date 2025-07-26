using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitQq.Abstractions;

namespace RabbitQq;

internal sealed class RabbitContext : IRabbitContext
{
    internal ConcurrentDictionary<string, RabbitPipeline> Dictionary { get; set; } = new();
    private readonly ConnectionFactory _connectionFactory;

    internal readonly ILogger<IRabbitContext>? _logger;
    internal IConnection? Connection { get; private set; }
    
    internal RabbitContext(ConnectionFactory connectionFactory, ILogger<IRabbitContext>? logger)
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

    public IRabbitPipeline? GetPipeline(string exchange)
    {
        if (Dictionary.TryGetValue(exchange, out var value) is false)
        {
            _logger?.LogWarning($"No pipeline with exchange {exchange} is registered.");
            return null;
        }

        return value;
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