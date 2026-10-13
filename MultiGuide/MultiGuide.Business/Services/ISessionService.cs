using MultiGuide.Business.Models;

namespace MultiGuide.Business.Services;

public interface ISessionService
{
    /// <summary>
    /// Tạo session mới cho khách (US-03).
    /// Ném NotFoundException nếu areaCode không hợp lệ.
    /// Ném BusinessValidationException nếu languageCode không được khu hỗ trợ.
    /// </summary>
    Task<SessionInfo> CreateSessionAsync(
        string areaCode,
        string languageCode,
        CancellationToken ct = default);
}
