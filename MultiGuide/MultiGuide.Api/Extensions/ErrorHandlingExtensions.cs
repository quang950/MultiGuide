using MultiGuide.Api.Middleware;

namespace MultiGuide.Api.Extensions;

public static class ErrorHandlingExtensions
{
    /// <summary>Đăng ký GlobalExceptionHandler vào DI (phải gọi trước AddControllers).</summary>
    public static IServiceCollection AddErrorHandlingServices(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    /// <summary>Đăng ký middleware xử lý lỗi và correlation ID vào pipeline.</summary>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseExceptionHandler();
        return app;
    }
}
