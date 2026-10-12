namespace MultiGuide.Domain.Entities;

public sealed class Payment
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string? ProviderRef { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public VisitorSession Session { get; set; } = null!;
    public AccessPass? AccessPass { get; set; }
}
