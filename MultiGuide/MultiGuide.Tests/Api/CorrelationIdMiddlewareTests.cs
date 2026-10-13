using Microsoft.AspNetCore.Http;
using MultiGuide.Api.Middleware;

namespace MultiGuide.Tests.Api;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoHeaderProvided_GeneratesNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        // Không set X-Correlation-Id trong request

        string? capturedId = null;
        var next = new RequestDelegate(_ =>
        {
            capturedId = context.TraceIdentifier;
            return Task.CompletedTask;
        });
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert – phải sinh mới và gán vào TraceIdentifier
        Assert.NotNull(capturedId);
        Assert.NotEmpty(capturedId);
        Assert.True(Guid.TryParse(capturedId, out _), "Correlation ID phải là GUID hợp lệ khi tự sinh.");
    }

    [Fact]
    public async Task InvokeAsync_HeaderProvided_KeepsExistingCorrelationId()
    {
        // Arrange
        const string existingId = "my-custom-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = existingId;

        string? capturedId = null;
        var next = new RequestDelegate(_ =>
        {
            capturedId = context.TraceIdentifier;
            return Task.CompletedTask;
        });
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert – phải giữ nguyên ID từ client
        Assert.Equal(existingId, capturedId);
    }
}
