using Options;
using RabbitMQ.Client.Events;

namespace RabbitQq.Abstractions;

public interface IRabbitPipeline
{
    Task RegisterReceiverAsync(string receiverName, ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler);
    Task DisposeReceiverAsync(string receiverName);
    Task BasicPublishAsync(string route, string body);
}
