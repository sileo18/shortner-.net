using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[Route("api")]
[ApiController]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UrlController> _logger;

    public UrlController(IUrlService urlService, IUserRepository userRepository, ILogger<UrlController> logger)
    {
        _urlService = urlService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public class CreateUserRequest
    {
        public required string Username { get; set; }
    }

    public class ShortenUrlRequest
    {
        public required Guid UserId { get; set; }
        public required string OriginalUrl { get; set; }
        public required string UserPrefix {get; set;}
    }

    [HttpPost("users")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest("Username is required.");
        }

        var user = new User(request.Username);
        await _userRepository.CreateUserAsync(user);

        _logger.LogInformation("User created: {UserId} - {Username}", user.Id, user.Username);
        return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, new { user.Id, user.Username });
    }

    [HttpGet("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid userId)
    {
        var user = await _userRepository.GetUserAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpPost("url/shorten")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request, [FromServices] IOptions<AppSettings> appSettings)
    {
        if (string.IsNullOrEmpty(request.OriginalUrl))
        {
            return BadRequest("Original URL is required.");
        }

        // Valida se usuário existe
        var userExists = await _userRepository.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            return NotFound("User not found.");
        }

        var shortCode = await _urlService.ShortenUrlAsync(request.OriginalUrl, request.UserId, request.UserPrefix);
        var baseUrl = appSettings.Value.BaseUrl;
        var shareableUrl = $"{baseUrl}/{shortCode}";

        return Ok(new { 
            ShortCode = shortCode,
            ShareableUrl = shareableUrl
        });
    }

    [HttpGet("url/{shortCode}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToOriginal([FromRoute] string shortCode)
    {
        _logger.LogInformation("Received request to redirect short code: {ShortCode}", shortCode);

        var originalUrl = await _urlService.ResolveAndTrackAsync(shortCode);

        if (originalUrl == null)
        {
            return NotFound("Short URL not found.");
        }

        return Redirect(originalUrl);
    }

    [HttpGet("url/{shortCode}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUrlDetails([FromRoute] string shortCode)
    {
        _logger.LogInformation("Received request to get URL details for short code: {ShortCode}", shortCode);

        var url = await _urlService.GetUrlDetailsAsync(shortCode);

        if (url == null)
        {
            return NotFound("URL not found.");
        }

        return Ok(new 
        { 
            url.Id,
            url.OriginalUrl,
            url.CreatedAt,
            url.ExpiresAt,
            ClickCount = await _urlService.GetClickCountAsync(shortCode)
        });
    }

    [HttpGet("user/{userId}/urls")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserUrls([FromRoute] Guid userId)
    {
        var userExists = await _userRepository.UserExistsAsync(userId);
        if (!userExists)        
            return NotFound("User not found.");

        var urls = await _urlService.GetUrlsByCodesAsync(userId);
        return Ok(urls);
    }

    [HttpGet("url/{shortCode}/clicks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClickCount([FromRoute] string shortCode)
    {
        var clickCount = await _urlService.GetClickCountAsync(shortCode);
        return Ok(new { ShortCode = shortCode, ClickCount = clickCount });
    }

    [HttpGet("user/{userId}/link-codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLinkCodes([FromRoute] Guid userId)
    {
        var userExists = await _userRepository.UserExistsAsync(userId);
        if (!userExists)        
            return NotFound("User not found.");

        var codes = await _urlService.GetUserLinkCodesAsync(userId);
        return Ok(codes);
    }
}