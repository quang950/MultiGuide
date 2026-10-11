namespace MultiGuide.Business.Models;

public sealed record AreaInfo(string Code, string Name, decimal PriceUsd, int PassTtlHours);
public sealed record LanguageInfo(string Code, string Name);
public sealed record SessionInfo(Guid SessionId, string AreaCode, string LanguageCode, DateTime StartedAt);
