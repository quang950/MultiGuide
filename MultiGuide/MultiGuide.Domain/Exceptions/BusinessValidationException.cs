using MultiGuide.Domain.Constants;

namespace MultiGuide.Domain.Exceptions;

/// <summary>
/// Ném khi dữ liệu đầu vào không hợp lệ theo quy tắc nghiệp vụ.
/// → HTTP 422 với code VALIDATION_ERROR.
/// </summary>
public sealed class BusinessValidationException : DomainException
{
    /// <summary>Chi tiết từng lỗi validation: key là tên trường, value là mô tả lỗi.</summary>
    public IReadOnlyDictionary<string, string> Errors { get; }

    public BusinessValidationException(string message, Dictionary<string, string> errors)
        : base(ErrorCodes.ValidationError, 422, message)
    {
        Errors = errors;
    }

    // Tiện ích khi chỉ có một lỗi đơn giản
    public BusinessValidationException(string field, string errorMessage)
        : this($"Validation failed: {errorMessage}", new Dictionary<string, string> { [field] = errorMessage }) { }
}
