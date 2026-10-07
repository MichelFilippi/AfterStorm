using AfterStorm.Domain.Entities;

namespace AfterStorm.Application.Interfaces;

public interface IMatchRepository
{
    Task<bool> ExistsByReplayHashAsync(string replayHash, CancellationToken cancellationToken = default);
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
}
