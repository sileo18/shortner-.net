using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
public class RedirectController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly ILogger<RedirectController> _logger;

    public RedirectController(IUrlService urlService, ILogger<RedirectController> logger)
    {
        _urlService = urlService;
        _logger = logger;
    }

    [HttpGet("{shortCode}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToUrl([FromRoute] string shortCode)
    {
        _logger.LogInformation("Received redirect request for short code: {ShortCode}", shortCode);

        var originalUrl = await _urlService.ResolveAndTrackAsync(shortCode);

        if (originalUrl == null)
        {
            return NotFound("Short URL not found.");
        }

        return Redirect(originalUrl);
    }
}
