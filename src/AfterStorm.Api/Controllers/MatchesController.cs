using System.Text.Json;
using AfterStorm.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AfterStorm.Api.Controllers;

[ApiController]
[Route("api/matches")]
public sealed class MatchesController(IMatchRepository matchRepository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetByIdAsync(id, cancellationToken);
        if (match is null)
            return NotFound();

        return Ok(new
        {
            match.Id,
            match.ExternalReplayId,
            match.Map,
            match.GameMode,
            match.PlayedAt,
            match.DurationSeconds,
            match.GameBuild,
            match.ProtocolBuild,
            match.ProtocolFallback,
            Players = match.Players.OrderBy(x => x.Team).ThenBy(x => x.ReplayPlayerId).Select(player => new
            {
                player.ReplayPlayerId,
                player.BattleTag,
                player.ToonHandle,
                player.Hero,
                player.Team,
                player.Won,
                player.Takedowns,
                player.SoloKills,
                player.Deaths,
                player.Assists,
                player.HeroDamage,
                player.SiegeDamage,
                player.DamageTaken,
                player.Healing,
                player.SelfHealing,
                player.ExperienceContribution,
                player.TimeSpentDead,
                player.RegenGlobes,
                Talents = player.Talents.OrderBy(x => x.Tier).Select(t => new
                {
                    t.Tier,
                    t.InternalName,
                    t.DisplayName,
                    t.SelectedAtSeconds
                }),
                DeathEvents = player.DeathEvents.OrderBy(x => x.Seconds).Select(d => new
                {
                    d.Seconds,
                    d.SoloDeath,
                    d.X,
                    d.Y,
                    Killers = JsonSerializer.Deserialize<string[]>(d.KillersJson) ?? []
                })
            }),
            Teams = match.Teams.OrderBy(x => x.Team).Select(team => new
            {
                team.Team,
                team.TeamId,
                team.IsWinner,
                team.Level,
                team.TotalXp,
                team.HeroXp,
                team.MinionXp,
                team.StructureXp,
                team.TrickleXp,
                LevelEvents = team.LevelEvents.OrderBy(x => x.Seconds).Select(x => new { x.Level, x.Seconds }),
                XpSnapshots = team.XpSnapshots.OrderBy(x => x.Seconds).Select(x => new
                {
                    x.Seconds,
                    x.Level,
                    x.HeroXp,
                    x.MinionXp,
                    x.StructureXp,
                    x.TrickleXp,
                    x.CreepXp
                })
            }),
            Timeline = match.TimelineEvents.OrderBy(x => x.Seconds).Select(x => new
            {
                x.EventType,
                x.Seconds,
                x.Team,
                x.UnitName,
                x.Level,
                x.SoloDeath
            })
        });
    }
}
