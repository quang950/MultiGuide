namespace MultiGuide.Domain.Exceptions;

/// <summary>
/// Base class cho tất cả business exception của MultiGuide.
/// Middleware sẽ bắt và chuyển thành ProblemDetails (RFC 7807).
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Mã lỗi ổn định, trả về trong trường ProblemDetails.code.</summary>
    public string ErrorCode { get; }

    /// <summary>HTTP status code tương ứng.</summary>
    public int StatusCode { get; }

    protected DomainException(string errorCode, int statusCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
