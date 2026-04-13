public class Url
{
    public string Id { get; set; } = string.Empty; // Short code (ex: "abc123")
    public string OriginalUrl { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // Opcional: para URLs com expiração
    public int AccessCount { get; set; } // Opcional: contar acessos
}