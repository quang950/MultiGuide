namespace MultiGuide.Domain.Entities;

public sealed class PoiTranslation
{
    public long Id { get; set; }
    public long PoiId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? AudioPath { get; set; }
    public int? AudioDurationSec { get; set; }
    public string? AudioChecksum { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Poi Poi { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
