using Options;
using RabbitMQ.Client.Events;

namespace RabbitQq.Abstractions;

public interface IRabbitPipeline : IAsyncDisposable
{
    Task RegisterReceiverAsync(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler);
}
