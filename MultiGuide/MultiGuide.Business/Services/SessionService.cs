using MultiGuide.Business.Interfaces;
using MultiGuide.Business.Models;
using MultiGuide.Domain.Constants;
using MultiGuide.Domain.Entities;
using MultiGuide.Domain.Exceptions;

namespace MultiGuide.Business.Services;

public sealed class SessionService(
    IAreaRepository areaRepo,
    ILanguageRepository languageRepo,
    ISessionRepository sessionRepo,
    IClock clock) : ISessionService
{
    public async Task<SessionInfo> CreateSessionAsync(
        string areaCode,
        string languageCode,
        CancellationToken ct = default)
    {
        // Kiểm tra khu tồn tại và active
        var area = await areaRepo.GetByCodeAsync(areaCode, ct);
        if (area is null || !area.IsActive)
            throw new NotFoundException(ErrorCodes.AreaNotFound, $"Area '{areaCode}' not found or inactive.");

        // Kiểm tra ngôn ngữ có được khu hỗ trợ (active và có ít nhất 1 POI có bản dịch)
        var supportedLanguages = await languageRepo.GetActiveLanguagesByAreaCodeAsync(areaCode, ct);
        var isLanguageSupported = supportedLanguages.Any(l =>
            string.Equals(l.Code, languageCode, StringComparison.OrdinalIgnoreCase));

        if (!isLanguageSupported)
            throw new BusinessValidationException(
                nameof(languageCode),
                $"Language '{languageCode}' is not supported for area '{areaCode}'.");

        var session = new VisitorSession
        {
            Id = Guid.NewGuid(),
            AreaId = area.Id,
            LanguageCode = languageCode,
            StartedAt = clock.UtcNow,
        };

        var saved = await sessionRepo.CreateAsync(session, ct);
        return new SessionInfo(saved.Id, area.Code, saved.LanguageCode, saved.StartedAt);
    }
}
