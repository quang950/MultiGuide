namespace MultiGuide.Api.Middleware;

/// <summary>
/// Middleware sinh hoặc giữ nguyên header X-Correlation-Id trong mỗi request.
/// ID được gắn vào HttpContext.TraceIdentifier và trả lại trong response header.
/// Không log correlation ID ở đây; Serilog enricher sẽ đọc từ HttpContext.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        // Lấy correlation ID từ request hoặc tự sinh mới
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Gắn vào TraceIdentifier để middleware và handler khác đọc được
        context.TraceIdentifier = correlationId;

        // Trả correlation ID về client trong response header
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
