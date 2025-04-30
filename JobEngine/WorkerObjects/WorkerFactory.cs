using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;

namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

/// <summary>
/// This class is responsible for creating worker instances.
/// </summary>
public class WorkerFactory
{
    /// <summary>
    /// Creates a worker instance based on the type provided.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="workQueue"></param>
    /// <param name="token"></param>
    /// <param name="databaseService"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IWorker CreateWorker(string type, WorkQueue<IWorkItem> workQueue, CancellationToken token, IDatabaseService databaseService, ILogger? logger = null)
    {
        return type switch
        {
            nameof(UserProcessingWorker) => new UserProcessingWorker(workQueue, token, databaseService, logger),
            nameof(FinancialDataWorker) => new FinancialDataWorker(workQueue, token, databaseService, logger),
            nameof(MessageProcessingWorker) => new MessageProcessingWorker(workQueue, token, databaseService, logger),
            _ => throw new ArgumentException("Unknown worker type")
        };
    }
}
