using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiGuide.Business.Interfaces;
using MultiGuide.Data.Repositories;
using MultiGuide.Data.Seeding;

namespace MultiGuide.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySQL(connectionString, mysql => mysql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Infrastructure
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<DbSeeder>();

        // Repositories
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        return services;
    }
}
