using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using StackExchange.Redis;

namespace BackgroundJobCodingChallenge.JobEngine;

/// <summary>
/// This interface defines the contract for processing various types of jobs.
/// </summary>
public interface IJobProcess
{
    /// <summary>
    /// Processes messages from a queue service.
    /// </summary>
    /// <param name="queueService"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ProcessQueueMessages(IQueueService<IWorkItem> queueService, CancellationToken cancellationToken);

    /// <summary>
    /// Processes messages from a cursor cache.
    /// </summary>
    /// <param name="triggerService"></param>
    /// <param name="connectionMultiplexer"></param>
    /// <param name="httpClient"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ProcessCursorData(ITriggerService triggerService, IConnectionMultiplexer connectionMultiplexer, HttpClient httpClient, CancellationToken cancellationToken);

    /// <summary>
    /// Processes user upload data from a database service.
    /// </summary>
    /// <param name="databaseService"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ProcessUserUploadData(IDatabaseService databaseService, CancellationToken cancellationToken);
}
