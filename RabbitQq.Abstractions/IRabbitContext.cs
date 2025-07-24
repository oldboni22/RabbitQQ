using Microsoft.Extensions.Logging;

namespace RabbitQq.Abstractions;

public interface IRabbitContext : IAsyncDisposable
{
    IRabbitPipeline? GetPipeline(string exchange);
}