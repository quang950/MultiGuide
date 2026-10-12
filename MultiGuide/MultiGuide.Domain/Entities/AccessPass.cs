namespace MultiGuide.Domain.Entities;

public sealed class AccessPass
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public long PaymentId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = null!;
    public VisitorSession Session { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}
