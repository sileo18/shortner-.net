using Microsoft.AspNetCore.Http.HttpResults;

public interface IUrlService
{
    Task<string> ShortenUrlAsync(string originalUrl, Guid userId, string userPrefix);
    Task<string?> ResolveAndTrackAsync(string shortCode);
    Task<IEnumerable<string>> GetUrlsByCodesAsync(Guid userId);
    Task<int> GetClickCountAsync(string shortCode);
    Task<IEnumerable<string>> GetUserLinkCodesAsync(Guid userId);
    Task<Url?> GetUrlDetailsAsync(string shortCode);
}

public class UrlService : IUrlService
{
    private readonly IUrlRepository _repository;

    private readonly ILogger<UrlService> _logger;

    public UrlService(IUrlRepository repository, ILogger<UrlService> logger)
    {
        _repository = repository;
        _logger = logger;   
    }

    public async Task<IEnumerable<string>> GetUserLinkCodesAsync(Guid userId)
    {
        return await _repository.GetUserLinkCodesAsync(userId);
    }
    public async Task<IEnumerable<string>> GetUrlsByCodesAsync(Guid userId)
    {
        var userCodes = await _repository.GetUserLinkCodesAsync(userId);
        return await _repository.GetUrlsByCodesAsync(userCodes);
    }

    public async Task<string> ShortenUrlAsync(string originalUrl, Guid userId, string userPrefix)
    {
        var code = Guid.NewGuid().ToString().Substring(0, 6);
        var shortCode = userPrefix + "-" +  code;

        var url = new Url
        {
            Id = shortCode,
            OriginalUrl = originalUrl,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        await _repository.SaveUrlAsync(url);
        return shortCode;
    }

    public async Task<string?> ResolveAndTrackAsync(string shortCode)
    { 

        string? originalUrl = await _repository.GetOriginalUrlAsync(shortCode);

        if(originalUrl != null)
        {
            // Executa em background para não travar o redirecionamento
            _ = _repository.IncrementsClickAsync(shortCode);      
        }  

        return originalUrl;
    }

    public async Task<int> GetClickCountAsync(string shortCode)
    {
        return await _repository.GetClickCountAsync(shortCode);
    }

    public async Task<Url?> GetUrlDetailsAsync(string shortCode)
    {
        _logger.LogInformation("Fetching URL details for short code: {ShortCode}", shortCode);
        return await _repository.GetUrlAsync(shortCode);
    }
}