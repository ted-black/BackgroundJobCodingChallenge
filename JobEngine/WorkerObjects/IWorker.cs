using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

/// <summary>
/// This interface defines the contract for worker classes.
/// </summary>
public interface IWorker
{
    /// <summary>
    /// Starts the worker process.
    /// </summary>
    /// <param name="reportPerformance"></param>
    /// <param name="idleTimeout"></param>
    /// <returns></returns>
    Task StartAsync(Action<IWorkItem?, TimeSpan?, bool> reportPerformance, TimeSpan idleTimeout);
}
