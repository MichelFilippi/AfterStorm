using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using AfterStorm.Application.Interfaces;
using AfterStorm.Application.Models;
using Microsoft.Extensions.Options;

namespace AfterStorm.Infrastructure.Replay;

public sealed class StormReplayParser(IOptions<HotsParserOptions> options) : IReplayParser
{
    private static readonly int[] TalentTiers = [1, 4, 7, 10, 13, 16, 20];
    private readonly HotsParserOptions _options = options.Value;

    public async Task<ParsedReplay> ParseAsync(string replayPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(replayPath))
            throw new FileNotFoundException("Replay file not found.", replayPath);

        var currentDirectory = Directory.GetCurrentDirectory();
        var python = ResolvePath(_options.PythonExecutable, currentDirectory);
        var script = ResolvePath(_options.ScriptPath, currentDirectory);
        var workingDirectory = ResolvePath(_options.WorkingDirectory, currentDirectory);

        if (!File.Exists(python))
            throw new InvalidOperationException($"Python executable was not found: {python}");
        if (!File.Exists(script))
            throw new InvalidOperationException($"hots-parser main.py was not found: {script}");
        if (!Directory.Exists(workingDirectory))
            throw new InvalidOperationException($"hots-parser working directory was not found: {workingDirectory}");

        var outputDirectory = Path.Combine(Path.GetTempPath(), "AfterStorm", "parser", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);

        try
        {
            await RunParserAsync(python, script, workingDirectory, outputDirectory, replayPath, cancellationToken);
            return await ReadOutputAsync(outputDirectory, cancellationToken);
        }
        finally
        {
            try { Directory.Delete(outputDirectory, recursive: true); }
            catch { /* Temp cleanup must never fail the import. */ }
        }
    }

    private static async Task RunParserAsync(
        string python,
        string script,
        string workingDirectory,
        string outputDirectory,
        string replayPath,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = python,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(script);
        startInfo.ArgumentList.Add("--dump-payloads");
        startInfo.ArgumentList.Add("--output-dir");
        startInfo.ArgumentList.Add(outputDirectory);
        startInfo.ArgumentList.Add(replayPath);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"hots-parser failed with exit code {process.ExitCode}.\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
        }
    }

    private static async Task<ParsedReplay> ReadOutputAsync(string outputDirectory, CancellationToken cancellationToken)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };

        var replayInfo = await ReadJsonAsync<ReplayInfoPayload>(Path.Combine(outputDirectory, "replayinfo.json"), jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("replayinfo.json was not generated.");
        var players = await ReadJsonAsync<List<PlayerPayload>>(Path.Combine(outputDirectory, "players.json"), jsonOptions, cancellationToken) ?? [];
        var stats = await ReadJsonAsync<List<GeneralStatsPayload>>(Path.Combine(outputDirectory, "generalstats.json"), jsonOptions, cancellationToken) ?? [];
        var teams = await ReadJsonAsync<List<TeamStatsPayload>>(Path.Combine(outputDirectory, "teamgeneralstats.json"), jsonOptions, cancellationToken) ?? [];
        var timeline = await ReadJsonAsync<TimelinePayload>(Path.Combine(outputDirectory, "timeline.json"), jsonOptions, cancellationToken);

        var statsByPlayer = stats
            .Where(x => x.PlayerId is not null)
            .ToDictionary(x => x.PlayerId!.Value);

        var parsedPlayers = players.Select(player =>
        {
            statsByPlayer.TryGetValue(player.PlayerId, out var stat);
            var talents = (stat?.PickedTalents ?? [])
                .Select((talent, index) => new ParsedTalent(
                    Tier: index < TalentTiers.Length ? TalentTiers[index] : 0,
                    InternalName: talent.TalentName ?? string.Empty,
                    DisplayName: talent.TalentName ?? string.Empty,
                    GameLoop: talent.Seconds * 16,
                    SelectedAtSeconds: talent.Seconds))
                .ToList();

            var deaths = (stat?.DeathsList ?? [])
                .Select(death => new ParsedDeath(
                    death.Seconds,
                    death.SoloDeath,
                    death.X,
                    death.Y,
                    death.Killers ?? []))
                .ToList();

            return new ParsedPlayer(
                player.PlayerId,
                player.BattleTag ?? player.Name ?? string.Empty,
                player.ToonHandle ?? string.Empty,
                player.Hero ?? string.Empty,
                player.Team,
                player.GameResult == 1,
                stat?.SoloKill,
                stat?.Takedowns,
                stat?.SoloKill,
                stat?.Deaths,
                stat?.Assists,
                stat?.HeroDamage,
                stat?.SiegeDamage,
                stat?.DamageTaken,
                stat?.Healing,
                stat?.SelfHealing,
                stat?.ExperienceContribution,
                stat?.TimeSpentDead,
                stat?.RegenGlobes,
                talents,
                deaths);
        }).ToList();

        var parsedTeams = teams.Select(team => new ParsedTeam(
            team.Team,
            team.TeamId ?? team.Team.ToString(CultureInfo.InvariantCulture),
            team.IsWinner,
            team.Level,
            team.TotalXp,
            team.TotalHeroXp,
            team.TotalMinionXp,
            team.TotalStructureXp,
            team.TotalTrickleXp,
            (team.LevelEvents ?? []).Select(x => new ParsedTeamLevelEvent(x.Level, x.Seconds)).ToList(),
            (team.PeriodicXpBreakdown ?? []).Select(x => new ParsedTeamXpSnapshot(
                (int)Math.Round(x.Seconds),
                x.Level,
                x.HeroXp,
                x.MinionXp,
                x.StructureXp,
                x.TrickleXp,
                x.CreepXp)).ToList()
        )).ToList();

        var parsedTimeline = (timeline?.Tl ?? [])
            .OrderBy(x => x.Seconds)
            .Select(x => new ParsedTimelineEvent(
                x.Event ?? "unknown",
                x.Seconds,
                x.Team,
                x.UnitName,
                x.Level,
                x.SoloDeath))
            .ToList();

        var playedAt = DateTimeOffset.TryParse(
            replayInfo.StartTime,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var parsedDate)
            ? parsedDate
            : DateTimeOffset.UtcNow;

        var durationSeconds = replayInfo.GameLoops > 0
            ? (int)Math.Round(replayInfo.GameLoops / 16d)
            : parsedTimeline.LastOrDefault()?.Seconds ?? 0;

        return new ParsedReplay(
            replayInfo.Id ?? string.Empty,
            replayInfo.MapName ?? "Unknown",
            replayInfo.GameType ?? "Unknown",
            playedAt,
            durationSeconds,
            replayInfo.GameVersion,
            replayInfo.ProtocolBuild,
            replayInfo.ProtocolFallback,
            parsedPlayers,
            parsedTeams,
            parsedTimeline);
    }

    private static async Task<T?> ReadJsonAsync<T>(string path, JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            return default;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
    }

    private static string ResolvePath(string configuredPath, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return string.Empty;

        if (Path.IsPathRooted(configuredPath))
            return Path.GetFullPath(configuredPath);

        foreach (var start in new[] { baseDirectory, AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            for (var i = 0; directory is not null && i < 8; i++, directory = directory.Parent)
            {
                var candidate = Path.GetFullPath(Path.Combine(directory.FullName, configuredPath));
                if (File.Exists(candidate) || Directory.Exists(candidate))
                    return candidate;
            }
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, configuredPath));
    }

    private sealed class ReplayInfoPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? Id { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("gameLoops")]
        public int GameLoops { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("gameType")]
        public string? GameType { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("gameVersion")]
        public int GameVersion { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("mapName")]
        public string? MapName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("protocolBuild")]
        public int ProtocolBuild { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("protocolFallback")]
        public bool ProtocolFallback { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("startTime")]
        public string? StartTime { get; set; }
    }

    private sealed class PlayerPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("battleTag")]
        public string? BattleTag { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("gameResult")]
        public int GameResult { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("hero")]
        public string? Hero { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("playerId")]
        public int PlayerId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("team")]
        public int Team { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("toonHandle")]
        public string? ToonHandle { get; set; }
    }

    private sealed class GeneralStatsPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("playerId")]
        public int? PlayerId { get; set; }
        public int? Assists { get; set; }
        public int? Deaths { get; set; }
        public int? SoloKill { get; set; }
        public int? Takedowns { get; set; }
        public int? HeroDamage { get; set; }
        public int? SiegeDamage { get; set; }
        public int? DamageTaken { get; set; }
        public int? Healing { get; set; }
        public int? SelfHealing { get; set; }
        public int? ExperienceContribution { get; set; }
        public int? TimeSpentDead { get; set; }
        public int? RegenGlobes { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("pickedTalents")]
        public List<TalentPayload>? PickedTalents { get; set; }
        public List<DeathPayload>? DeathsList { get; set; }

        // System.Text.Json maps the JSON property "deaths" to this explicit name.
        [System.Text.Json.Serialization.JsonPropertyName("deaths")]
        public List<DeathPayload>? DeathsPayload
        {
            get => DeathsList;
            set => DeathsList = value;
        }
    }

    private sealed class TalentPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("seconds")]
        public int Seconds { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("talent_name")]
        public string? TalentName { get; set; }
    }

    private sealed class DeathPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("killers")]
        public List<string>? Killers { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("seconds")]
        public int Seconds { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("soloDeath")]
        public bool SoloDeath { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("x")]
        public double? X { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("y")]
        public double? Y { get; set; }
    }

    private sealed class TeamStatsPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("isWinner")]
        public bool IsWinner { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("level")]
        public int Level { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("team")]
        public int Team { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("teamId")]
        public string? TeamId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalXP")]
        public double TotalXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalHeroXP")]
        public double TotalHeroXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalMinionXP")]
        public double TotalMinionXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalStructureXP")]
        public double TotalStructureXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalTrickleXP")]
        public double TotalTrickleXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("levelEvents")]
        public List<LevelEventPayload>? LevelEvents { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("periodicXPBreakdown")]
        public List<XpSnapshotPayload>? PeriodicXpBreakdown { get; set; }
    }

    private sealed class LevelEventPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("level")]
        public int Level { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("seconds")]
        public int Seconds { get; set; }
    }

    private sealed class XpSnapshotPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("CreepXP")]
        public double CreepXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("HeroXP")]
        public double HeroXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("MinionXP")]
        public double MinionXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("StructureXP")]
        public double StructureXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("TrickleXP")]
        public double TrickleXp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("level")]
        public int Level { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("seconds")]
        public double Seconds { get; set; }
    }

    private sealed class TimelinePayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("tl")]
        public List<TimelineEventPayload>? Tl { get; set; }
    }

    private sealed class TimelineEventPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("event")]
        public string? Event { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("seconds")]
        public int Seconds { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("team")]
        public int? Team { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("unitName")]
        public string? UnitName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("level")]
        public int? Level { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("soloDeath")]
        public bool? SoloDeath { get; set; }
    }
}
