using MultiGuide.Business.Interfaces;
using MultiGuide.Business.Models;
using MultiGuide.Domain.Constants;
using MultiGuide.Domain.Exceptions;

namespace MultiGuide.Business.Services;

public sealed class AreaService(IAreaRepository areaRepo, ILanguageRepository languageRepo) : IAreaService
{
    public async Task<AreaInfo> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var area = await areaRepo.GetByCodeAsync(code, ct);

        // Khu không tồn tại hoặc đã tắt → 404
        if (area is null || !area.IsActive)
            throw new NotFoundException(ErrorCodes.AreaNotFound, $"Area '{code}' not found or inactive.");

        return new AreaInfo(area.Code, area.Name, area.PriceUsd, area.PassTtlHours);
    }

    public async Task<IReadOnlyList<LanguageInfo>> GetLanguagesByAreaCodeAsync(
        string code,
        CancellationToken ct = default)
    {
        // Kiểm tra khu tồn tại và active trước
        var area = await areaRepo.GetByCodeAsync(code, ct);
        if (area is null || !area.IsActive)
            throw new NotFoundException(ErrorCodes.AreaNotFound, $"Area '{code}' not found or inactive.");

        var languages = await languageRepo.GetActiveLanguagesByAreaCodeAsync(code, ct);
        return languages.Select(l => new LanguageInfo(l.Code, l.Name)).ToList();
    }
}
