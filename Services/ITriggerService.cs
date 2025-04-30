namespace BackgroundJobCodingChallenge.Services;

public interface ITriggerService
{
	Task Subscribe(Action<CancellationToken> executeAsync);

	void Unsubscribe(Action<CancellationToken> executeAsync);

    Task WaitForTrigger(CancellationToken cancellationToken);
}
