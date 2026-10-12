namespace MultiGuide.Domain.Entities;

public sealed class Poi
{
    public long Id { get; set; }
    public long AreaId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int TriggerRadiusM { get; set; }
    public int Priority { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public TourArea Area { get; set; } = null!;
    public ICollection<PoiTranslation> Translations { get; } = [];
    public ICollection<PlayLog> PlayLogs { get; } = [];
}
