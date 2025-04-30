using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

namespace BackgroundJobCodingChallenge.Services;

public interface IQueueService<TMessage> where TMessage : IWorkItem
{
    Task QueueMessageAsync(int queueId, TMessage message, CancellationToken cancellationToken = default);

    Task SubscribeToMessages(int queueId, Action<TMessage, CancellationToken> processAsync);

    void UnsubscribeFromMessages(Action<TMessage, CancellationToken> processAsync);

    void ListenForMessages();
}
