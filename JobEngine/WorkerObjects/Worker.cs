namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This class is responsible for processing work items.
/// </summary>
/// <param name="workQueue"></param>
/// <param name="token"></param>
/// <param name="logger"></param>
public abstract class Worker(WorkQueue<IWorkItem> workQueue, CancellationToken token, ILogger? logger = null) : IWorker
{
    private const int MaxRetries = 3;
    private static readonly Stopwatch Timer = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="reportPerformance"></param>
    /// <param name="idleTimeout"></param>
    /// <returns></returns>
    public async Task StartAsync(Action<IWorkItem?, TimeSpan?, bool> reportPerformance, TimeSpan idleTimeout)
    {
        try
        {
            var idleTimer = Stopwatch.StartNew();

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (workQueue.TryTake(out IWorkItem? item, 1000)) // Wait 1 sec for new work
                {
                    Timer.Restart();
                    await ProcessItemAsync(item);
                    Timer.Stop();
                    reportPerformance(item, Timer.Elapsed, true);
                }
                else
                {
                    logger?.LogDebug("Worker idle, waiting for new work...");
                    await Task.Delay(500); // Avoid busy looping

                    // Check if idle time exceeds the threshold
                    if (token.IsCancellationRequested || idleTimer.Elapsed >= idleTimeout)
                    {
                        throw new OperationCanceledException("Worker idle timeout exceeded.");
                    }
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            logger?.LogDebug($"Worker canceled or expired. ex --> {ex.Message}");
        }
        catch (Exception ex)
        {
            logger?.LogError($"Worker encountered an error: {ex.Message}");
            reportPerformance(null, null, false);
        }
    }

    /// <summary>
    /// Processes the work item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual async Task ProcessItemAsync(IWorkItem? item)
    {
        logger?.LogDebug($"Processing {item} on Thread {Task.CurrentId}");
        await Task.Delay(new Random().Next(500, 2000)); // Simulate variable processing time
    }
}
