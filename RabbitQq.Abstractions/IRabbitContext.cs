using Microsoft.Extensions.Logging;

namespace RabbitQq.Abstractions;

public interface IRabbitContext : IAsyncDisposable
{
    ILogger? Logger { get; set; }
}