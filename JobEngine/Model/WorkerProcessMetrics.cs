namespace BackgroundJobCodingChallenge.JobEngine.Model;

/// <summary>
/// WorkerProcessMetrics is a class that holds metrics related to the worker process.
/// </summary>
public class WorkerProcessMetrics
{
    public int QueueSize { get; set; }
    public int ActiveWorkers { get; set; }
    public int FailedItems { get; set; }
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Creates a new instance of the WorkerProcessMetrics class.
    /// </summary>
    /// <param name="queueSize"></param>
    /// <param name="activeWorkers"></param>
    /// <param name="failedItems"></param>
    /// <param name="totalProcessed"></param>
    public WorkerProcessMetrics(int queueSize, int activeWorkers, int failedItems, int totalProcessed)
    {
        QueueSize = queueSize;
        ActiveWorkers = activeWorkers;
        FailedItems = failedItems;
        TotalProcessed = totalProcessed;
    }
}
