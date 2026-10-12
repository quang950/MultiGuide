namespace MultiGuide.Domain.Entities;

public sealed class PlayLog
{
    public long Id { get; set; }
    public long PoiId { get; set; }
    public Guid SessionId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public int ListenedSec { get; set; }
    public bool Completed { get; set; }
    public Poi Poi { get; set; } = null!;
    public VisitorSession Session { get; set; } = null!;
}
