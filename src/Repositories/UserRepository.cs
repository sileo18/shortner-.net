using System.Text.Json;
using StackExchange.Redis;

public interface IUserRepository
{
    Task CreateUserAsync(User user);
    Task<User?> GetUserAsync(Guid userId);
    Task<bool> UserExistsAsync(Guid userId);
}

public class UserRepository : IUserRepository
{
    private readonly IDatabase _db;
    private readonly string _userPrefix = "user:data";
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConnectionMultiplexer redis, ILogger<UserRepository> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task CreateUserAsync(User user)
    {
        var userKey = $"{_userPrefix}:{user.Id}";
        var json = JsonSerializer.Serialize(user);
        await _db.StringSetAsync(userKey, json);
        _logger.LogInformation("User created: {UserId} - {Username}", user.Id, user.Username);
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        var userKey = $"{_userPrefix}:{userId}";
        var data = await _db.StringGetAsync(userKey);

        if (data.IsNull)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        var user = JsonSerializer.Deserialize<User>((string)data!);
        _logger.LogInformation("User retrieved: {UserId}", userId);
        return user;
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var userKey = $"{_userPrefix}:{userId}";
        return await _db.KeyExistsAsync(userKey);
    }
}
