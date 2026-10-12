using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiGuide.Domain.Entities;

namespace MultiGuide.Data.Configurations;

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> b)
    {
        b.ToTable("languages");
        b.HasKey(x => x.Code);
        b.Property(x => x.Code).HasMaxLength(10);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
    }
}

public sealed class TourAreaConfiguration : IEntityTypeConfiguration<TourArea>
{
    public void Configure(EntityTypeBuilder<TourArea> b)
    {
        b.ToTable("tour_areas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.PriceUsd).HasPrecision(10, 2);
    }
}

public sealed class PoiConfiguration : IEntityTypeConfiguration<Poi>
{
    public void Configure(EntityTypeBuilder<Poi> b)
    {
        b.ToTable("pois");
        b.HasKey(x => x.Id);
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.HasIndex(x => new { x.AreaId, x.IsActive });
        b.HasIndex(x => new { x.Lat, x.Lng });
        b.HasOne(x => x.Area).WithMany(x => x.Pois).HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PoiTranslationConfiguration : IEntityTypeConfiguration<PoiTranslation>
{
    public void Configure(EntityTypeBuilder<PoiTranslation> b)
    {
        b.ToTable("poi_translations");
        b.HasKey(x => x.Id);
        b.Property(x => x.LanguageCode).HasMaxLength(10);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasColumnType("text");
        b.Property(x => x.AudioPath).HasMaxLength(500);
        b.Property(x => x.AudioChecksum).HasColumnType("char(64)");
        b.HasIndex(x => new { x.PoiId, x.LanguageCode }).IsUnique();
        b.HasOne(x => x.Poi).WithMany(x => x.Translations).HasForeignKey(x => x.PoiId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Language).WithMany().HasForeignKey(x => x.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class VisitorSessionConfiguration : IEntityTypeConfiguration<VisitorSession>
{
    public void Configure(EntityTypeBuilder<VisitorSession> b)
    {
        b.ToTable("visitor_sessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnType("char(36)");
        b.Property(x => x.LanguageCode).HasMaxLength(10);
        b.HasOne(x => x.Area).WithMany(x => x.VisitorSessions).HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Language>().WithMany().HasForeignKey(x => x.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PlayLogConfiguration : IEntityTypeConfiguration<PlayLog>
{
    public void Configure(EntityTypeBuilder<PlayLog> b)
    {
        b.ToTable("play_logs");
        b.HasKey(x => x.Id);
        b.Property(x => x.SessionId).HasColumnType("char(36)");
        b.Property(x => x.LanguageCode).HasMaxLength(10);
        b.HasIndex(x => new { x.PoiId, x.StartedAt });
        b.HasOne(x => x.Poi).WithMany(x => x.PlayLogs).HasForeignKey(x => x.PoiId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Session).WithMany(x => x.PlayLogs).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Language>().WithMany().HasForeignKey(x => x.LanguageCode).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.HasKey(x => x.Id);
        b.Property(x => x.SessionId).HasColumnType("char(36)");
        b.Property(x => x.Amount).HasPrecision(10, 2);
        b.Property(x => x.Currency).HasColumnType("char(3)");
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.ProviderRef).HasMaxLength(100);
        b.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.RequestHash).HasColumnType("char(64)").IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => new { x.SessionId, x.IdempotencyKey }).IsUnique();
        b.HasOne(x => x.Session).WithMany(x => x.Payments).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AccessPassConfiguration : IEntityTypeConfiguration<AccessPass>
{
    public void Configure(EntityTypeBuilder<AccessPass> b)
    {
        b.ToTable("access_passes");
        b.HasKey(x => x.Id);
        b.Property(x => x.SessionId).HasColumnType("char(36)");
        b.Property(x => x.TokenHash).HasColumnType("char(64)");
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.PaymentId).IsUnique();
        b.HasOne(x => x.Session).WithMany(x => x.AccessPasses).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Payment).WithOne(x => x.AccessPass).HasForeignKey<AccessPass>(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> b)
    {
        b.ToTable("admin_users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).HasMaxLength(255);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.PasswordHash).HasMaxLength(255);
        b.Property(x => x.Role).HasMaxLength(30);
    }
}
