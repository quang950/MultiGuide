using MultiGuide.Domain.Entities;

namespace MultiGuide.Business.Interfaces;

public interface ISessionRepository
{
    /// <summary>Lưu session mới vào DB và trả về session sau khi lưu (có Id đã gán).</summary>
    Task<VisitorSession> CreateAsync(VisitorSession session, CancellationToken ct = default);
}
