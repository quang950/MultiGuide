using MultiGuide.Domain.Constants;

namespace MultiGuide.Domain.Exceptions;

/// <summary>
/// Ném khi không tìm thấy resource hoặc resource bị tắt (is_active = false).
/// → HTTP 404 với code từ <see cref="ErrorCodes"/>.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string message)
        : base(errorCode, 404, message) { }
}
