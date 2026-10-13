using MultiGuide.Domain.Entities;

namespace MultiGuide.Business.Interfaces;

public interface ILanguageRepository
{
    /// <summary>
    /// Trả các ngôn ngữ is_active có ít nhất 1 POI active có bản dịch trong khu <paramref name="areaCode"/>.
    /// Trả danh sách rỗng nếu khu không có ngôn ngữ nào đáp ứng.
    /// </summary>
    Task<IReadOnlyList<Language>> GetActiveLanguagesByAreaCodeAsync(
        string areaCode,
        CancellationToken ct = default);
}
