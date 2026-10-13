using Microsoft.EntityFrameworkCore;
using MultiGuide.Business.Interfaces;
using MultiGuide.Domain.Entities;

namespace MultiGuide.Data.Repositories;

public sealed class LanguageRepository(AppDbContext db) : ILanguageRepository
{
    public async Task<IReadOnlyList<Language>> GetActiveLanguagesByAreaCodeAsync(
        string areaCode,
        CancellationToken ct = default)
    {
        // Lấy ngôn ngữ is_active có ít nhất 1 POI active trong khu có bản dịch đó
        var languages = await db.Languages
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Where(l => db.PoiTranslations.Any(pt =>
                pt.LanguageCode == l.Code &&
                pt.Poi.IsActive &&
                pt.Poi.Area.Code == areaCode))
            .OrderBy(l => l.Code)
            .ToListAsync(ct);

        return languages;
    }
}
