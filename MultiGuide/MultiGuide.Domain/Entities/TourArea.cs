namespace MultiGuide.Domain.Entities;

public sealed class TourArea
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public double CenterLat { get; set; }
    public double CenterLng { get; set; }
    public int RadiusM { get; set; }
    public decimal PriceUsd { get; set; }
    public int PassTtlHours { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Poi> Pois { get; } = [];
    public ICollection<VisitorSession> VisitorSessions { get; } = [];
}
