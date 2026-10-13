namespace MultiGuide.Business.Models;

/// <summary>DTO nội bộ Business cho thông tin khu tham quan (US-01).</summary>
public sealed record AreaInfo(
    string Code,
    string Name,
    decimal PriceUsd,
    int PassTtlHours);

/// <summary>DTO nội bộ Business cho ngôn ngữ (US-02).</summary>
public sealed record LanguageInfo(
    string Code,
    string Name);

/// <summary>DTO nội bộ Business cho session vừa tạo (US-03).</summary>
public sealed record SessionInfo(
    Guid SessionId,
    string AreaCode,
    string LanguageCode,
    DateTime StartedAt);
