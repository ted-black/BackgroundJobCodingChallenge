using BackgroundJobCodingChallenge.JobEngine.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackgroundJobCodingChallenge.JobEngine;

/// <summary>
/// This class handles the retrieval of financial data using a cursor from Redis.
/// Performance has not been considered here as we haven't tested the code yet.
/// </summary>
public class CursorCache(IConnectionMultiplexer _redis, HttpClient client, string endpoint, ILogger? logger)
{
    public async Task<FinancialDataResponse?> RetrieveFinancialDataAsync()
    {
        IDatabase _db = _redis.GetDatabase();
        string? cursor = await _db.StringGetAsync("financial_cursor");

        logger.LogInformation("Fetching data with cursor: {Cursor}", cursor);
        var response = await client.GetAsync($"{endpoint}?cursor={cursor}");

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("API failure. Status: {StatusCode}, Cursor: {Cursor}", response.StatusCode, cursor);
            throw new EntryPointNotFoundException("API failure");
        }

        string json = await response.Content.ReadAsStringAsync();
        FinancialDataResponse? result = JsonConvert.DeserializeObject<FinancialDataResponse>(json);
        cursor = result?.NextCursor;

        await _db.StringSetAsync("financial_cursor", cursor); // Update cursor in Redis

        logger.LogInformation("Processed cursor: {Cursor}", cursor);

        logger.LogInformation("Completed fetching financial data.");

        return result;
    }
}