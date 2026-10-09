using System.Security.Cryptography;
using System.Text.Json;
using AfterStorm.Application.Interfaces;
using AfterStorm.Application.Models;
using AfterStorm.Domain.Entities;

namespace AfterStorm.Application.Services;

public sealed class ReplayImportService(
    IReplayStorage replayStorage,
    IReplayParser replayParser,
    IMatchRepository matchRepository)
{
    public async Task<ReplayUploadResult> ImportAsync(
        Stream replayStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        await replayStream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        if (await matchRepository.ExistsByReplayHashAsync(hash, cancellationToken))
            throw new InvalidOperationException("This replay has already been imported.");

        memory.Position = 0;
        var path = await replayStorage.SaveAsync(memory, fileName, cancellationToken);
        var parsed = await replayParser.ParseAsync(path, cancellationToken);

        var match = new Match
        {
            ReplayHash = hash,
            ExternalReplayId = parsed.ExternalReplayId,
            Map = parsed.Map,
            GameMode = parsed.GameMode,
            PlayedAt = parsed.PlayedAt,
            DurationSeconds = parsed.DurationSeconds,
            GameBuild = parsed.GameBuild,
            ProtocolBuild = parsed.ProtocolBuild,
            ProtocolFallback = parsed.ProtocolFallback,
            Players = parsed.Players.Select(player => new MatchPlayer
            {
                ReplayPlayerId = player.ReplayPlayerId,
                BattleTag = player.BattleTag,
                ToonHandle = player.ToonHandle,
                Hero = player.Hero,
                Team = player.Team,
                Won = player.Won,
                Kills = player.Kills,
                Takedowns = player.Takedowns,
                SoloKills = player.SoloKills,
                Deaths = player.Deaths,
                Assists = player.Assists,
                HeroDamage = player.HeroDamage,
                SiegeDamage = player.SiegeDamage,
                DamageTaken = player.DamageTaken,
                Healing = player.Healing,
                SelfHealing = player.SelfHealing,
                ExperienceContribution = player.ExperienceContribution,
                TimeSpentDead = player.TimeSpentDead,
                RegenGlobes = player.RegenGlobes,
                Talents = player.Talents.Select(talent => new TalentSelection
                {
                    Tier = talent.Tier,
                    InternalName = talent.InternalName,
                    DisplayName = talent.DisplayName,
                    GameLoop = talent.GameLoop,
                    SelectedAtSeconds = talent.SelectedAtSeconds
                }).ToList(),
                DeathEvents = player.DeathEvents.Select(death => new PlayerDeath
                {
                    Seconds = death.Seconds,
                    SoloDeath = death.SoloDeath,
                    X = death.X,
                    Y = death.Y,
                    KillersJson = JsonSerializer.Serialize(death.Killers)
                }).ToList()
            }).ToList(),
            Teams = parsed.Teams.Select(team => new MatchTeam
            {
                Team = team.Team,
                TeamId = team.TeamId,
                IsWinner = team.IsWinner,
                Level = team.Level,
                TotalXp = team.TotalXp,
                HeroXp = team.HeroXp,
                MinionXp = team.MinionXp,
                StructureXp = team.StructureXp,
                TrickleXp = team.TrickleXp,
                LevelEvents = team.LevelEvents.Select(x => new TeamLevelEvent
                {
                    Level = x.Level,
                    Seconds = x.Seconds
                }).ToList(),
                XpSnapshots = team.XpSnapshots.Select(x => new TeamXpSnapshot
                {
                    Seconds = x.Seconds,
                    Level = x.Level,
                    HeroXp = x.HeroXp,
                    MinionXp = x.MinionXp,
                    StructureXp = x.StructureXp,
                    TrickleXp = x.TrickleXp,
                    CreepXp = x.CreepXp
                }).ToList()
            }).ToList(),
            TimelineEvents = parsed.Timeline.Select(x => new MatchTimelineEvent
            {
                EventType = x.EventType,
                Seconds = x.Seconds,
                Team = x.Team,
                UnitName = x.UnitName,
                Level = x.Level,
                SoloDeath = x.SoloDeath
            }).ToList()
        };

        await matchRepository.AddAsync(match, cancellationToken);

        return new ReplayUploadResult(
            match.Id,
            hash,
            match.ExternalReplayId,
            match.Map,
            match.DurationSeconds,
            match.Players.Count,
            match.Players.Sum(x => x.Talents.Count),
            match.TimelineEvents.Count);
    }
}
