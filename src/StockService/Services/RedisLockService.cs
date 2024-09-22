using StackExchange.Redis;

namespace StockService.Services;

public class RedisLockService
{
    private readonly IDatabase _database;

    public RedisLockService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
    {
        return await _database.LockTakeAsync(key, Environment.MachineName, expiry);
    }

    public async Task<bool> ReleaseLockAsync(string key)
    {
        return await _database.LockReleaseAsync(key, Environment.MachineName);
    }
    // Kilidi periyodik olarak yenilemek için  
    public async Task RenewLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(expiry / 2); // Expiry süresinin yarısında kilidi yenile
            await _database.LockExtendAsync(key, Environment.MachineName, expiry);
        }
    }
}


