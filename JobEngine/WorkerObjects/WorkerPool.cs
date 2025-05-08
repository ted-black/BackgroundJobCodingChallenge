namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This class is responsible for managing a pool of workers.
/// </summary>
/// <param name="workQueue"></param>
/// <param name="failedItems"></param>
/// <param name="maxConcurrency"></param>
/// <param name="databaseService"></param>
/// <param name="logger"></param>
public class WorkerPool(WorkQueue<IWorkItem> workQueue, ConcurrentQueue<IWorkItem> failedItems, int maxConcurrency, IDatabaseService databaseService,  ILogger? logger = null)
{
    private readonly ConcurrentBag<Task> _workers = new();
    private readonly ConcurrentBag<CancellationTokenSource> _workerTokens = new();
    private readonly object _lock = new();
    private int _totalItemsProcessed;

    public int TotalItemsProcessed => _totalItemsProcessed;
    public int CurrentWorkerCount => _workers.Count;
    public int MaxConcurrency => maxConcurrency;

    /// <summary>
    /// Creates a new instance of the WorkerPool class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void AddWorker<T>() where T : IWorker
    {
        lock (_lock)
        {
            if (_workers.Count >= MaxConcurrency) return; // Prevent exceeding max workers

            var cts = new CancellationTokenSource();
            T worker = (T)WorkerFactory.CreateWorker(typeof(T).Name, workQueue, cts.Token, databaseService, logger);
            var task = Task.Run(()=> worker.StartAsync(ReportPerformance, new TimeSpan(0,0, 5)));

            _workerTokens.Add(cts);
            _workers.Add(task);

            Log($"Worker added. Current count: {_workers.Count}");
        }
    }

    /// <summary>
    /// Reports the performance of the worker.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="processingTime"></param>
    /// <param name="succeeded"></param>
    private void ReportPerformance(IWorkItem? item, TimeSpan? processingTime, bool succeeded)
    {
        if (succeeded)
        {
            Interlocked.Increment(ref _totalItemsProcessed);
        }
        else
        {
            Log($"Dispatcher received failed item: {(item == null ? "item is null" : item)}");
            if (item != null)
            {
                failedItems.Enqueue(item);
            }
        }
    }

    /// <summary>
    /// Removes a worker from the pool.
    /// </summary>
    public void RemoveWorker()
    {
        lock (_lock)
        {
            if (_workers.IsEmpty) return;

            var tokenSource = _workerTokens.FirstOrDefault();
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                _workerTokens.TryTake(out _);
            }

            _workers.TryTake(out _);
            Log($"Worker removed. Current count: {_workers.Count}");
        }
    }

    /// <summary>
    /// Waits for all workers to complete their tasks or for the cancellation token to be triggered.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task WaitForCompletionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Create a task that completes when the cancellation token is triggered
            var cancellationTask = Task.Run(() =>
            {
                cancellationToken.WaitHandle.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
            });

            // Wait for either all workers to complete or the cancellation token to be triggered
            await Task.WhenAny(Task.WhenAll(_workers), cancellationTask);

            // If the cancellation token was triggered, throw an exception
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            Log("WaitForCompletionAsync was canceled.");
            throw; // Re-throw the exception to propagate cancellation
        }
    }

    /// <summary>
    /// Shuts down the worker pool.
    /// </summary>
    /// <param name="force"></param>
    public void Shutdown(bool force = false)
    {
        Log("Shutting down worker pool...");
        foreach (var token in _workerTokens)
        {
            token.Cancel();
        }

        if (!force)
        {
            Task.WaitAll(_workers.ToArray());
        }
    }

    /// <summary>
    /// Logs a message if a logger is provided.
    /// </summary>
    /// <param name="message"></param>
    private void Log(string message)
    {
        if (logger != null)
        {
            logger.LogDebug(message);
        }
    }
}
