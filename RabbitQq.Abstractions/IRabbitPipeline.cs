using Options;
using RabbitMQ.Client.Events;

namespace RabbitQq.Abstractions;

public interface IRabbitPipeline
{
    Task RegisterReceiverAsync(ReceiverQueueOptions options, AsyncEventHandler<BasicDeliverEventArgs> handler);
    Task DisposeReceiverAsync(string queueName);
    Task BasicPublishAsync(string route, string body);
}
