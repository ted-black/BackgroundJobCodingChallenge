namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects;

using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using System.Collections.Concurrent;

/// <summary>
/// A thread-safe work queue that uses a BlockingCollection.
/// </summary>
/// <typeparam name="T"></typeparam>
public class WorkQueue<T> where T : IWorkItem
{
    /// <summary>
    /// The queue that holds the work items.
    /// </summary>
    private readonly BlockingCollection<T> _queue = new(100); // Limit queue size to prevent overflow.

    /// <summary>
    /// Number of items in the queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// The number of items in the queue.
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item) => _queue.Add(item);

    /// <summary>
    /// Removes and returns an item from the queue.
    /// </summary>
    /// <returns></returns>
    public T Take() => _queue.Take(); // Blocks until an item is available.

    /// <summary>
    /// Removes and returns an item from the queue, or returns false if the queue is empty.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public bool TryTake(out T? item, int timeout) => _queue.TryTake(out item, timeout);

    /// <summary>
    /// Signals that no more items will be added to the queue.
    /// </summary>
    public void CompleteAdding() => _queue.CompleteAdding();
}
