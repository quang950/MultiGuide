using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MultiGuide.Domain.Exceptions;

namespace MultiGuide.Api.Middleware;

/// <summary>
/// Xử lý tất cả exception chưa được bắt, chuyển thành ProblemDetails chuẩn RFC 7807.
/// Trường mở rộng bắt buộc: "code" (mã lỗi ổn định) và "traceId" (correlation ID).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        var (statusCode, errorCode, title, detail, errors) = exception switch
        {
            DomainException de => (de.StatusCode, de.ErrorCode, GetTitle(de.StatusCode), de.Message,
                de is BusinessValidationException bve ? bve.Errors : null),
            _ => (500, "INTERNAL_ERROR", "Internal Server Error",
                "An unexpected error occurred. Please try again later.", null)
        };

        // Không log stack trace cho lỗi domain (đã xử lý có chủ đích)
        if (exception is DomainException)
            logger.LogInformation("Business error {ErrorCode} on {Path}: {Message}",
                errorCode, httpContext.Request.Path, exception.Message);
        else
            logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = statusCode;

        // Tạo ProblemDetails theo RFC 7807 + trường mở rộng code, traceId, errors
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["code"] = errorCode;
        problem.Extensions["traceId"] = traceId;
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken: cancellationToken);
        return true;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        402 => "Payment Required",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        423 => "Locked",
        429 => "Too Many Requests",
        _ => "Internal Server Error",
    };
}
