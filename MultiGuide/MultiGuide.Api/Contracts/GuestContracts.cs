using System.ComponentModel.DataAnnotations;

namespace MultiGuide.Api.Contracts;

public sealed record AreaResponse(string Code, string Name, decimal PriceUsd, int PassTtlHours);
public sealed record LanguageResponse(string Code, string Name);

public sealed class CreateSessionRequest
{
    [Required, StringLength(50)]
    public string AreaCode { get; init; } = "";

    [Required, RegularExpression("^[a-zA-Z]{2,3}(-[a-zA-Z0-9]{2,8})?$")]
    public string LanguageCode { get; init; } = "";
}

public sealed record SessionResponse(
    Guid SessionId, string AreaCode, string LanguageCode, DateTime StartedAt);
