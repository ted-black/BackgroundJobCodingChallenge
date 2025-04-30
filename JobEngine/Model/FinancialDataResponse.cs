using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

namespace BackgroundJobCodingChallenge.JobEngine.Model;

/// <summary>
/// Represents a response from the financial data service.
/// </summary>
public class FinancialDataResponse : IWorkItem
{
    /// <summary>
    /// The data returned from the financial data service.
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
}
