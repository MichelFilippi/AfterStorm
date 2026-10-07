using AfterStorm.Application.Models;

namespace AfterStorm.Application.Interfaces;

public interface IReplayParser
{
    Task<ParsedReplay> ParseAsync(string replayPath, CancellationToken cancellationToken = default);
}
