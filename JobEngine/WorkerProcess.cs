using BackgroundJobCodingChallenge.JobEngine.Model;
using BackgroundJobCodingChallenge.JobEngine.WorkerObjects;
using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace BackgroundJobCodingChallenge.JobEngine;

/// <summary>
/// This class is responsible for managing the distribution of work items to worker threads.
/// </summary>
public abstract class WorkerProcess
{
    protected readonly WorkQueue<IWorkItem> _workQueue = new();
    protected readonly ConcurrentQueue<IWorkItem> _failedItems = new();
    protected readonly WorkerPool _workerPool;
    protected readonly int _scalingThreshold;
    protected readonly int _retryLimit;
    protected readonly ILogger? _logger;

    /// <summary>
    /// Creates a new instance of the WorkerProcess class.
    /// </summary>
    /// <param name="databaseService"></param>
    /// <param name="maxConcurrency"></param>
    /// <param name="scalingThreshold"></param>
    /// <param name="retryLimit"></param>
    /// <param name="logger"></param>
    protected WorkerProcess(IDatabaseService databaseService, int maxConcurrency = 10, int scalingThreshold = 5, int retryLimit = 3, ILogger? logger = null)
    {
        _workerPool = new WorkerPool(_workQueue, _failedItems, maxConcurrency, databaseService, logger);
        _scalingThreshold = scalingThreshold;
        _retryLimit = retryLimit;
        _logger = logger;
    }

    /// <summary>
    /// Distributes work items to workers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="workload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task DistributeWorkAsync<T>(IWorkItem[] workload, CancellationToken cancellationToken) where T : IWorker
    {
        foreach (IWorkItem item in workload)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _workQueue.Add(item);

            // Adjust worker count in batches
            if (_workQueue.Count % _scalingThreshold == 0)
            {
                AdjustWorkerCount<T>();
            }
        }

        // Final adjustment after all items are added
        AdjustWorkerCount<T>();

        await WaitForCompletionAsync(cancellationToken);

        // Retry failed items
        await RetryFailedItemsAsync(cancellationToken);
    }

    /// <summary>
    /// Distributes a single work item to workers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected Task DistributeWorkAsync<T>(IWorkItem item) where T : IWorker
    {
        _workQueue.Add(item);

        // Adjust worker count in batches
        if (_workQueue.Count % _scalingThreshold == 0)
        {
            AdjustWorkerCount<T>();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Distributes work items from a cursor cache to workers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cursorCache"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task DistributeWorkAsync<T>(CursorCache cursorCache, CancellationToken cancellationToken) where T : IWorker
    {
        FinancialDataResponse? response = await cursorCache.RetrieveFinancialDataAsync();

        while (response != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _workQueue.Add(response);
            // Adjust worker count in batches
            if (_workQueue.Count % _scalingThreshold == 0)
            {
                AdjustWorkerCount<T>();
            }
            response = await cursorCache.RetrieveFinancialDataAsync();
        }

        // Final adjustment after all items are added
        AdjustWorkerCount<T>();

        await WaitForCompletionAsync(cancellationToken);

        // Retry failed items
        await RetryFailedItemsAsync(cancellationToken);
    }

    /// <summary>
    /// Waits for all tasks to complete.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task WaitForCompletionAsync(CancellationToken cancellationToken)
    {
        Log(JsonConvert.SerializeObject(GetMetrics()));
        await _workerPool.WaitForCompletionAsync(cancellationToken);
        Log("All tasks processed.");
    }

    /// <summary>
    /// Adjusts the number of workers based on the current queue size and scaling threshold.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected void AdjustWorkerCount<T>() where T : IWorker
    {
        int queueSize = _workQueue.Count;
        int targetWorkers = Math.Min(_workerPool.MaxConcurrency, Math.Max(1, queueSize / _scalingThreshold));

        while (_workerPool.CurrentWorkerCount < targetWorkers)
        {
            _workerPool.AddWorker<T>();
        }

        while (_workerPool.CurrentWorkerCount > targetWorkers)
        {
            _workerPool.RemoveWorker();
        }
    }

    /// <summary>
    /// Retries failed items in the queue.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task RetryFailedItemsAsync(CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (!_failedItems.IsEmpty && retryCount < _retryLimit)
        {
            retryCount++;
            Log($"Retrying failed items (Attempt {retryCount}/{_retryLimit})...");

            var itemsToRetry = new List<IWorkItem>();
            while (_failedItems.TryDequeue(out var item))
            {
                itemsToRetry.Add(item);
            }

            foreach (var item in itemsToRetry)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _workQueue.Add(item);
            }

            await WaitForCompletionAsync(cancellationToken);
        }

        if (!_failedItems.IsEmpty)
        {
            Log("Some items failed after maximum retry attempts.");
        }
    }

    /// <summary>
    /// Shuts down the dispatcher and all workers.
    /// </summary>
    /// <param name="force"></param>
    protected void Shutdown(bool force = false)
    {
        Log("Shutting down dispatcher...");
        _workerPool.Shutdown(force);
    }

    /// <summary>
    /// Gets the current metrics of the worker process.
    /// </summary>
    /// <returns></returns>
    protected WorkerProcessMetrics GetMetrics()
    {
        return new (_workQueue.Count, _workerPool.CurrentWorkerCount, _failedItems.Count, _workerPool.TotalItemsProcessed);
    }

    /// <summary>
    /// Logs a message to the logger if available.
    /// </summary>
    /// <param name="message"></param>
    protected void Log(string message)
    {
        if (_logger != null)
        {
            _logger.LogTrace(message);
        }
    }
}
