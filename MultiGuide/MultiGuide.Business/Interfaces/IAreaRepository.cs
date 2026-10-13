using MultiGuide.Domain.Entities;

namespace MultiGuide.Business.Interfaces;

public interface IAreaRepository
{
    /// <summary>Lấy khu theo code (không phân biệt hoa thường). Trả null nếu không tồn tại.</summary>
    Task<TourArea?> GetByCodeAsync(string code, CancellationToken ct = default);
}
