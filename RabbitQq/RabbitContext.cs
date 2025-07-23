using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitQq.Abstractions;

namespace RabbitQq;

public sealed class RabbitContext : IRabbitContext
{
    private readonly ConnectionFactory _connectionFactory;
    public ILogger? Logger { get; set; }

    internal IConnection? Connection { get; private set; }

    internal RabbitContext(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    internal async Task InitializeAsync()
    {
        try
        {
            Connection = await _connectionFactory.CreateConnectionAsync();
            Logger?.LogInformation("RabbitMq connection created successfully.");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "An exception occured while opening connection.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
        }
    }
}