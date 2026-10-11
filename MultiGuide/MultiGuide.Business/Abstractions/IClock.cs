namespace MultiGuide.Business.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
