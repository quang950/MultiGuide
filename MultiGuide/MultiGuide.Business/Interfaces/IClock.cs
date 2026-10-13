namespace MultiGuide.Business.Interfaces;

/// <summary>
/// Trừu tượng hoá đồng hồ hệ thống để Service không gọi DateTime.UtcNow trực tiếp,
/// giúp dễ mock trong unit test.
/// </summary>
public interface IClock
{
    /// <summary>Trả thời điểm hiện tại theo UTC.</summary>
    DateTime UtcNow { get; }
}
