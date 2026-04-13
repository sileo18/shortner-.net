using System.Text.Json;
using StackExchange.Redis;

public interface IUrlRepository
{
    Task SaveUrlAsync(Url url);
    Task<string?> GetOriginalUrlAsync(string shortCode);
    Task IncrementsClickAsync(string shortCode);
    Task<IEnumerable<string>> GetUserLinkCodesAsync(Guid userId);
    Task<IEnumerable<string>> GetUrlsByCodesAsync(IEnumerable<string> codes);
    Task<int> GetClickCountAsync(string shortCode);
}

public class UrlRepository : IUrlRepository
{

    private readonly IDatabase _db;
    private readonly string _shortPrefix = "short";
    private const string _userPrefix = "user:{0}:links";
    private readonly ILogger<UrlRepository> _logger;

    public UrlRepository(IConnectionMultiplexer redis, ILogger<UrlRepository> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }
    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var data = await _db.StringGetAsync($"{_shortPrefix}:{shortCode}");

        _logger.LogInformation("Fetching URL for short code {ShortCode}: {Data}", shortCode, data);

        if (data.IsNull)
        {
            _logger.LogWarning("Short code {ShortCode} not found in Redis.", shortCode);
            return null;
        }

        return (string)data!;
    }

    public async Task SaveUrlAsync(Url url)
    {
       var userKey = string.Format(_userPrefix, url.UserId);

       var batch = _db.CreateTransaction();

       batch.StringSetAsync($"{_shortPrefix}:{url.Id}", url.OriginalUrl!);
       batch.SetAddAsync(userKey, url.Id);

        if (url.ExpiresAt.HasValue)
        {
            var ttl = url.ExpiresAt.Value - DateTime.UtcNow;
            batch.KeyExpireAsync($"{_shortPrefix}:{url.Id}", ttl);
        }

        var executed = await batch.ExecuteAsync();
        _logger.LogInformation("Transaction executed: {Executed}", executed);
    }

    public async Task IncrementsClickAsync(string shortCode)
    {
        // INCR stats:abc123:clicks
        await _db.StringIncrementAsync($"stats:{shortCode}:clicks");
    }

    public async Task<IEnumerable<string>> GetUserLinkCodesAsync(Guid userId)
    {
        var userKey = string.Format(_userPrefix, userId);
        var members = await _db.SetMembersAsync(userKey);
        return members.Select(m => m.ToString());
    }

    public async Task<IEnumerable<string>> GetUrlsByCodesAsync(IEnumerable<string> codes)
    {
        if (!codes.Any()) return Enumerable.Empty<string>();

        // MGET para buscar vários de uma vez
        var keys = codes.Select(c => (RedisKey)$"{_shortPrefix}:{c}").ToArray();
        var results = await _db.StringGetAsync(keys);

        _logger.LogInformation("Fetched URLs for {Count} codes", results.Length);

        return results
            .Where(r => !r.IsNull)
            .Select(r => (string)r!);
    }

    public async Task<int> GetClickCountAsync(string shortCode)
    {
        var clicks = await _db.StringGetAsync($"stats:{shortCode}:clicks");
        return clicks.IsNull ? 0 : (int)clicks!;
    }
}