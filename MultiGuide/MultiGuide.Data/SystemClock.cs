using MultiGuide.Business.Interfaces;

namespace MultiGuide.Data;

/// <summary>Implement IClock dùng đồng hồ hệ thống thật (DateTime.UtcNow).</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
