using MultiGuide.Business.Services;

namespace MultiGuide.Api.Extensions;

public static class GuestServiceExtensions
{
    /// <summary>
    /// Đăng ký tất cả Business Service của nhóm Khách vào DI container.
    /// Program.cs chỉ gọi hàm này theo quy ước kiến trúc.
    /// </summary>
    public static IServiceCollection AddGuestServices(this IServiceCollection services)
    {
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<ISessionService, SessionService>();
        return services;
    }
}
