using MultiGuide.Business.Models;

namespace MultiGuide.Business.Services;

public interface IAreaService
{
    /// <summary>Lấy thông tin khu theo code (US-01). Ném NotFoundException nếu không tìm thấy hoặc is_active = false.</summary>
    Task<AreaInfo> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Lấy danh sách ngôn ngữ đang bật có ít nhất 1 POI active có bản dịch trong khu (US-02).</summary>
    Task<IReadOnlyList<LanguageInfo>> GetLanguagesByAreaCodeAsync(string code, CancellationToken ct = default);
}
