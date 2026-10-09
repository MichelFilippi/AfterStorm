using AfterStorm.Application.Interfaces;
using AfterStorm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfterStorm.Infrastructure.Persistence;

public sealed class MatchRepository(AfterStormDbContext dbContext) : IMatchRepository
{
    public Task<bool> ExistsByReplayHashAsync(string replayHash, CancellationToken cancellationToken = default) =>
        dbContext.Matches.AnyAsync(x => x.ReplayHash == replayHash, cancellationToken);

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        dbContext.Matches.Add(match);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Matches
            .AsNoTracking()
            .Include(x => x.Players).ThenInclude(x => x.Talents)
            .Include(x => x.Players).ThenInclude(x => x.DeathEvents)
            .Include(x => x.Teams).ThenInclude(x => x.LevelEvents)
            .Include(x => x.Teams).ThenInclude(x => x.XpSnapshots)
            .Include(x => x.TimelineEvents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
