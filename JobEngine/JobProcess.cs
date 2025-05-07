using BackgroundJobCodingChallenge.JobEngine.WorkerObjects;
using BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;
using BackgroundJobCodingChallenge.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BackgroundJobCodingChallenge.JobEngine;

/// <summary>
/// This class is responsible for processing various types of jobs.
/// Add process methods for each job type.
/// </summary>
/// <param name="databaseService"></param>
/// <param name="maxConcurrency"></param>
/// <param name="scalingThreshold"></param>
/// <param name="retryLimit"></param>
/// <param name="logger"></param>
public class JobProcess(IDatabaseService databaseService, int maxConcurrency = 10, int scalingThreshold = 5, int retryLimit = 3, ILogger? logger = null)
{
    /// <summary>
    /// Processes messages from a queue service.
    /// </summary>
    /// <param name="queueService"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessQueueMessages(IQueueService<IWorkItem> queueService, CancellationToken cancellationToken)
    {
        WorkerProcess<MessageProcessingWorker> workerProcess = new(databaseService, maxConcurrency, scalingThreshold, retryLimit, logger);
        Action<IWorkItem, CancellationToken> processAction = async (message, token) =>
        {
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                // Process the message
                await workerProcess.DistributeWorkAsync(message);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
                logger?.LogInformation("Processing cancelled.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                logger?.LogError(ex, "Error processing message.");
            }
        };

        // Subscribe to messages
        await queueService.SubscribeToMessages(1, processAction);

        // Listen for messages
        queueService.ListenForMessages();

        // Wait for completion
        await workerProcess.WaitForCompletionAsync(cancellationToken);

        // Retry failed items
        await workerProcess.RetryFailedItemsAsync(cancellationToken);

        // Unsubscribe from messages
        queueService.UnsubscribeFromMessages(processAction);
    }

    /// <summary>
    /// Processes cursor data from a trigger service.
    /// </summary>
    /// <param name="triggerService"></param>
    /// <param name="connectionMultiplexer"></param>
    /// <param name="httpClient"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessCursorData(ITriggerService triggerService, IConnectionMultiplexer connectionMultiplexer, HttpClient httpClient, CancellationToken cancellationToken)
    {
        // Create the cursor cache
        CursorCache cursorCache = new CursorCache(connectionMultiplexer, httpClient, Environment.GetEnvironmentVariable(Constant.WorkerProcess.FinancialSystemEndpoint) ?? "", logger);

        // Create the worker process
        WorkerProcess<FinancialDataWorker> workerProcess = new(databaseService, maxConcurrency, scalingThreshold, retryLimit, logger);

        // Define the action to be performed for each cursor
        Action<CancellationToken> cursorAction = async (token) =>
        {
            await workerProcess.DistributeWorkAsync(cursorCache, token);
        };

        // Subscribe to cursor data
        await triggerService.Subscribe(cursorAction);

        // Wait for trigger
        await triggerService.WaitForTrigger(cancellationToken);

        // Unsubscribe from cursor data
        triggerService.Unsubscribe(cursorAction);
    }

    /// <summary>
    /// Processes user upload data.
    /// </summary>
    /// <param name="databaseService"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ProcessUserUploadData(IDatabaseService databaseService, CancellationToken cancellationToken)
    {
        // Create the worker process
        WorkerProcess<UserProcessingWorker> workerProcess = new(databaseService, maxConcurrency, scalingThreshold, retryLimit, logger);

        // Get the user info from the database
        IEnumerable<UserInfo> userInfo = await databaseService.GetUploadedAsync<UserInfo>();

        // Check if the user info is null or empty
        if (userInfo != null)
        {
            // Process the user info
            await workerProcess.DistributeWorkAsync([.. userInfo], cancellationToken);
        }
    }

}
