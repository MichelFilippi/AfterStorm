using AfterStorm.Application.Interfaces;
using AfterStorm.Application.Models;

namespace AfterStorm.Infrastructure.Replay;

/// <summary>
/// Placeholder for the first development milestone.
/// Next step: invoke the heroprotocol-based parser and deserialize its normalized JSON output.
/// </summary>
public sealed class StormReplayParser : IReplayParser
{
    public Task<ParsedReplay> ParseAsync(string replayPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(replayPath))
            throw new FileNotFoundException("Replay file not found.", replayPath);

        throw new NotImplementedException(
            "Replay parser is not connected yet. The next milestone will integrate heroprotocol and map TalentChosen events.");
    }
}
