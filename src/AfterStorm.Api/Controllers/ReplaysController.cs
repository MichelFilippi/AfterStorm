using AfterStorm.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AfterStorm.Api.Controllers;

[ApiController]
[Route("api/replays")]
public sealed class ReplaysController(ReplayImportService replayImportService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile replay, CancellationToken cancellationToken)
    {
        if (replay.Length == 0)
            return BadRequest(new { error = "Replay file is empty." });

        if (!Path.GetExtension(replay.FileName).Equals(".StormReplay", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only .StormReplay files are accepted." });

        try
        {
            await using var stream = replay.OpenReadStream();
            var result = await replayImportService.ImportAsync(stream, replay.FileName, cancellationToken);
            return Created($"/api/matches/{result.MatchId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
