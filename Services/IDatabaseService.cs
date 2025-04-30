namespace BackgroundJobCodingChallenge.Services;

public interface IDatabaseService
{
	Task<TEntity> GetAsync<TEntity>(int id);

	Task<IEnumerable<TEntity>> GetUploadedAsync<TEntity>();

    Task CreateAsync<TEntity>(TEntity entity);

	Task CreateAsync<TEntity>(IEnumerable<TEntity> entities);

	Task<TEntity> UpdateAsync<TEntity>(TEntity entities);

	Task<TEntity> UpdateAsync<TEntity>(IEnumerable<TEntity> entities);

	Task DeleteAsync<TEntity>(TEntity entities);

	Task DeleteAsync<TEntity>(IEnumerable<TEntity> entities);
}
