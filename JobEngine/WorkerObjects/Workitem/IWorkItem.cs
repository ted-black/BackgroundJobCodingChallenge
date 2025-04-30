namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

/// <summary>
/// This interface represents a work item that can be processed by a worker.
/// </summary>
public interface IWorkItem
{
    int Id { get; }
}
