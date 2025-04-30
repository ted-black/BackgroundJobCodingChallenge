using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

/// <summary>
/// This class is responsible for processing financial data jobs.
/// </summary>
/// <param name="workQueue"></param>
/// <param name="token"></param>
/// <param name="databaseService"></param>
/// <param name="logger"></param>
public class FinancialDataWorker(WorkQueue<IWorkItem> workQueue, CancellationToken token, IDatabaseService databaseService, ILogger? logger = null) : Worker(workQueue, token, logger)
{
    private static readonly object _lock = new object();

    /// <summary>
    /// Processes the financial data job.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task ProcessItemAsync(IWorkItem? item)
    {
        // Process financial data
        await databaseService.CreateAsync(item);
    }


    /// <summary>
    /// Writes the content to a file in a thread-safe manner.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    private void WriteToFile(string path, string content)
    {
        lock (_lock) // Ensures only one thread writes at a time
        {
            using (StreamWriter writer = new(path, append: true))
            {
                writer.WriteLineAsync(content);
            }
        }
    }
}
