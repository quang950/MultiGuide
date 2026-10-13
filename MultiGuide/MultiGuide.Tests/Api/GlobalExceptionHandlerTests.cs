using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiGuide.Api.Middleware;
using MultiGuide.Domain.Constants;
using MultiGuide.Domain.Exceptions;
using NSubstitute;
using System.Text.Json;

namespace MultiGuide.Tests.Api;

public sealed class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id-123";

        var exception = new NotFoundException(ErrorCodes.AreaNotFound, "Tour area not found.");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        Assert.Equal(404, root.GetProperty("status").GetInt32());
        Assert.Equal(ErrorCodes.AreaNotFound, root.GetProperty("code").GetString());
        Assert.Equal("test-trace-id-123", root.GetProperty("traceId").GetString());
        Assert.Equal("Tour area not found.", root.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_BusinessValidationException_Returns422WithErrors()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-422";

        var errors = new Dictionary<string, string>
        {
            ["language_code"] = "Language not supported."
        };
        var exception = new BusinessValidationException("Validation failed.", errors);

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        Assert.Equal(422, root.GetProperty("status").GetInt32());
        Assert.Equal(ErrorCodes.ValidationError, root.GetProperty("code").GetString());
        Assert.Equal("trace-422", root.GetProperty("traceId").GetString());
        Assert.True(root.TryGetProperty("errors", out var errorsProp));
        Assert.True(errorsProp.TryGetProperty("language_code", out _));
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_Returns500InternalError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-500";

        var exception = new InvalidOperationException("Something unexpected broke!");

        // Act
        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        Assert.Equal(500, root.GetProperty("status").GetInt32());
        Assert.Equal(ErrorCodes.InternalError, root.GetProperty("code").GetString());
        Assert.Equal("trace-500", root.GetProperty("traceId").GetString());
    }
}
