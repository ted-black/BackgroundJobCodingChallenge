using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

/// <summary>
/// This class is responsible for processing messages from a queue.
/// </summary>
/// <param name="workQueue"></param>
/// <param name="token"></param>
/// <param name="databaseService"></param>
/// <param name="logger"></param>
internal class MessageProcessingWorker(WorkQueue<IWorkItem> workQueue, CancellationToken token, IDatabaseService databaseService, ILogger? logger = null) : Worker(workQueue, token, logger)
{
    private static readonly object _lock = new object();

    /// <summary>
    /// Processes the message job.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task ProcessItemAsync(IWorkItem? item)
    {
        // Process message data
        await databaseService.CreateAsync(item);
    }
}
