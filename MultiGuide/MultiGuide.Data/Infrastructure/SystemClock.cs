using MultiGuide.Business.Abstractions;

namespace MultiGuide.Data.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}