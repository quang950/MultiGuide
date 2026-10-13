using Microsoft.EntityFrameworkCore;
using MultiGuide.Business.Interfaces;
using MultiGuide.Domain.Entities;

namespace MultiGuide.Data.Repositories;

public sealed class AreaRepository(AppDbContext db) : IAreaRepository
{
    public Task<TourArea?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        db.TourAreas
          .AsNoTracking()
          .FirstOrDefaultAsync(a => a.Code == code, ct);
}
