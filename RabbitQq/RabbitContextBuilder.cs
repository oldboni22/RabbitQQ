using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Options;
using RabbitMQ.Client;
using RabbitQq.Abstractions;

namespace RabbitQq;

public class RabbitContextBuilder
{
    private RabbitContextBuilder() {}

    private readonly ConcurrentDictionary<string, ExchangeDeclareOptions> _dictionary = new();
    
    private ILogger<IRabbitContext>? _logger;
    
    public static RabbitContextBuilder GetBuilder() => new RabbitContextBuilder();

    public RabbitContextBuilder AddPipeline(string exchangeName, ExchangeDeclareOptions options)
    {
        if (_dictionary.ContainsKey(exchangeName))
        {
            return this;
        }

        _dictionary.TryAdd(exchangeName, options);
        
        return this;
    }
    
    public RabbitContextBuilder WithLogger(ILogger<IRabbitContext> logger)
    {
        _logger = logger;
        return this;
    }

    public async Task<IRabbitContext> BuildAsync(ConnectionFactory connectionFactory)
    {
        var context = new RabbitContext(connectionFactory, _logger);
        await context.InitializeAsync();

        var dictionary = new ConcurrentDictionary<string, RabbitPipeline>();

        await InitializePipelines(context, dictionary);

        context.Dictionary = dictionary;

        return context;
    }

    private async Task InitializePipelines(RabbitContext context, ConcurrentDictionary<string, RabbitPipeline> dictionary)
    {
        await Parallel.ForEachAsync
        (_dictionary, CancellationToken.None, async (pair, token) =>
        {
            var pipeline = new RabbitPipeline(context, pair.Key, pair.Value);
            await pipeline.InitializeAsync();

            dictionary.TryAdd(pair.Key, pipeline);
        });
    }
    
}