using MultiGuide.Business.Abstractions;
using MultiGuide.Data.Infrastructure;

namespace MultiGuide.Api.Extensions;

public static class CommonServiceExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}