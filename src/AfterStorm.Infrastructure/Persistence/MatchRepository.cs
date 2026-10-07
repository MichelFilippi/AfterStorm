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
}
