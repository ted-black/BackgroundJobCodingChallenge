using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

/// <summary>
/// This class is responsible for processing user data jobs.
/// </summary>
/// <param name="workQueue"></param>
/// <param name="token"></param>
/// <param name="databaseService"></param>
/// <param name="logger"></param>
public class UserProcessingWorker(WorkQueue<IWorkItem> workQueue, CancellationToken token, IDatabaseService databaseService, ILogger? logger = null) : Worker(workQueue, token, logger)
{
    private static readonly object _lock = new();

    /// <summary>
    /// Processes the user data job.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task ProcessItemAsync(IWorkItem? item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "Work item cannot be null.");
        }
        // Process user data
        UserInfo userInfo = await databaseService.GetAsync<UserInfo>(item.Id);
        if (userInfo == null)
        {
            await databaseService.CreateAsync(item);
        }
        else
        {
            lock (_lock) // Ensures only one thread writes at a time
            {
                databaseService.UpdateAsync(item).Wait();
            }
        }
    }
}
