using System.ComponentModel.DataAnnotations;

namespace MultiGuide.Api.Contracts;

// ── US-01 ─────────────────────────────────────────────────────────────────────
public sealed record AreaResponse(
    string Code,
    string Name,
    decimal PriceUsd,
    int PassTtlHours);

// ── US-02 ─────────────────────────────────────────────────────────────────────
public sealed record LanguageResponse(
    string Code,
    string Name);

// ── US-03 ─────────────────────────────────────────────────────────────────────
public sealed class CreateSessionRequest
{
    [Required, StringLength(50)]
    public string AreaCode { get; init; } = "";

    /// <summary>Language code BCP-47: vi, en, ja, en-US, …</summary>
    [Required, RegularExpression(@"^[a-zA-Z]{2,3}(-[a-zA-Z0-9]{2,8})?$",
        ErrorMessage = "language_code must be a valid BCP-47 tag (e.g. vi, en, ja).")]
    public string LanguageCode { get; init; } = "";
}

public sealed record SessionResponse(
    Guid SessionId,
    string AreaCode,
    string LanguageCode,
    DateTime StartedAt);
