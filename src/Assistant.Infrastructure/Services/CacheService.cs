using StackExchange.Redis;
using System.Text.Json;

namespace Assistant.Infrastructure.Services;

public class CacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);

    public CacheService(string? redisConnectionString = null)
    {
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(redisConnectionString);
                _db = _redis.GetDatabase();
            }
            catch
            {
                // Redis не доступен, работаем без кэша
                _redis = null;
                _db = null;
            }
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (_db == null) return default;

        var value = await _db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (_db == null) return;

        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        if (_db == null) return false;

        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (_db == null) return false;

        return await _db.KeyExistsAsync(key);
    }
}
