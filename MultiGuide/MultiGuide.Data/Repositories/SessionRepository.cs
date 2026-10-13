using MultiGuide.Business.Interfaces;
using MultiGuide.Domain.Entities;

namespace MultiGuide.Data.Repositories;

public sealed class SessionRepository(AppDbContext db) : ISessionRepository
{
    public async Task<VisitorSession> CreateAsync(VisitorSession session, CancellationToken ct = default)
    {
        db.VisitorSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }
}
