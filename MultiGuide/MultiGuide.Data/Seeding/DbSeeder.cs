using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiGuide.Business.Interfaces;
using MultiGuide.Domain.Entities;

namespace MultiGuide.Data.Seeding;

public sealed class DbSeeder(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<DbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var language in new[]
        {
            new Language { Code = "vi", Name = "Tiếng Việt", IsActive = true },
            new Language { Code = "en", Name = "English", IsActive = true },
            new Language { Code = "ja", Name = "日本語", IsActive = true }
        })
        {
            if (!await db.Languages.AnyAsync(x => x.Code == language.Code, cancellationToken))
                db.Languages.Add(language);
        }

        await db.SaveChangesAsync(cancellationToken);

        var area = await db.TourAreas.FirstOrDefaultAsync(x => x.Code == "demo-area", cancellationToken);
        if (area is null)
        {
            area = new TourArea
            {
                Code = "demo-area",
                Name = "Khu demo",
                CenterLat = 10.7769,
                CenterLng = 106.7009,
                RadiusM = 500,
                PriceUsd = 2.00m,
                PassTtlHours = 24,
                IsActive = true
            };
            db.TourAreas.Add(area);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Pois.AnyAsync(x => x.AreaId == area.Id, cancellationToken))
        {
            for (var i = 1; i <= 6; i++)
            {
                db.Pois.Add(new Poi
                {
                    AreaId = area.Id,
                    Lat = area.CenterLat + i * 0.0005,
                    Lng = area.CenterLng,
                    TriggerRadiusM = 30,
                    Priority = 50,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Skipping admin seed because Seed__AdminEmail or Seed__AdminPassword is missing.");
            return;
        }

        if (!await db.AdminUsers.AnyAsync(x => x.Email == email, cancellationToken))
        {
            db.AdminUsers.Add(new AdminUser
            {
                Email = email,
                PasswordHash = passwordHasher.Hash(password),
                Role = "super_admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
