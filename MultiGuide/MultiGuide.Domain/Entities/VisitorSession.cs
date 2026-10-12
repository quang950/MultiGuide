namespace MultiGuide.Domain.Entities;

public sealed class VisitorSession
{
    public Guid Id { get; set; }
    public long AreaId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TourArea Area { get; set; } = null!;
    public ICollection<Payment> Payments { get; } = [];
    public ICollection<AccessPass> AccessPasses { get; } = [];
    public ICollection<PlayLog> PlayLogs { get; } = [];
}
