using System.Security.Cryptography;
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
            Map = parsed.Map,
            GameMode = parsed.GameMode,
            PlayedAt = parsed.PlayedAt,
            DurationSeconds = parsed.DurationSeconds,
            GameBuild = parsed.GameBuild,
            Players = parsed.Players.Select(player => new MatchPlayer
            {
                ReplayPlayerId = player.ReplayPlayerId,
                BattleTag = player.BattleTag,
                ToonHandle = player.ToonHandle,
                Hero = player.Hero,
                Team = player.Team,
                Won = player.Won,
                Kills = player.Kills,
                Deaths = player.Deaths,
                Assists = player.Assists,
                Talents = player.Talents.Select(talent => new TalentSelection
                {
                    Tier = talent.Tier,
                    InternalName = talent.InternalName,
                    DisplayName = talent.DisplayName,
                    GameLoop = talent.GameLoop,
                    SelectedAtSeconds = talent.SelectedAtSeconds
                }).ToList()
            }).ToList()
        };

        await matchRepository.AddAsync(match, cancellationToken);

        return new ReplayUploadResult(
            match.Id,
            hash,
            match.Map,
            match.Players.Count,
            match.Players.Sum(x => x.Talents.Count));
    }
}
