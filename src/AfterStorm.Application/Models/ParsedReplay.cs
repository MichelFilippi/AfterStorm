namespace AfterStorm.Application.Models;

public sealed record ParsedReplay(
    string Map,
    string GameMode,
    DateTimeOffset PlayedAt,
    int DurationSeconds,
    int GameBuild,
    IReadOnlyCollection<ParsedPlayer> Players);

public sealed record ParsedPlayer(
    int ReplayPlayerId,
    string BattleTag,
    string ToonHandle,
    string Hero,
    int Team,
    bool Won,
    int? Kills,
    int? Deaths,
    int? Assists,
    IReadOnlyCollection<ParsedTalent> Talents);

public sealed record ParsedTalent(
    int Tier,
    string InternalName,
    string DisplayName,
    int GameLoop,
    int SelectedAtSeconds);
