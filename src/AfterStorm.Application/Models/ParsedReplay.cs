namespace AfterStorm.Application.Models;

public sealed record ParsedReplay(
    string ExternalReplayId,
    string Map,
    string GameMode,
    DateTimeOffset PlayedAt,
    int DurationSeconds,
    int GameBuild,
    int ProtocolBuild,
    bool ProtocolFallback,
    IReadOnlyCollection<ParsedPlayer> Players,
    IReadOnlyCollection<ParsedTeam> Teams,
    IReadOnlyCollection<ParsedTimelineEvent> Timeline);

public sealed record ParsedPlayer(
    int ReplayPlayerId,
    string BattleTag,
    string ToonHandle,
    string Hero,
    int Team,
    bool Won,
    int? Kills,
    int? Takedowns,
    int? SoloKills,
    int? Deaths,
    int? Assists,
    int? HeroDamage,
    int? SiegeDamage,
    int? DamageTaken,
    int? Healing,
    int? SelfHealing,
    int? ExperienceContribution,
    int? TimeSpentDead,
    int? RegenGlobes,
    IReadOnlyCollection<ParsedTalent> Talents,
    IReadOnlyCollection<ParsedDeath> DeathEvents);

public sealed record ParsedTalent(
    int Tier,
    string InternalName,
    string DisplayName,
    int GameLoop,
    int SelectedAtSeconds);

public sealed record ParsedDeath(
    int Seconds,
    bool SoloDeath,
    double? X,
    double? Y,
    IReadOnlyCollection<string> Killers);

public sealed record ParsedTeam(
    int Team,
    string TeamId,
    bool IsWinner,
    int Level,
    double TotalXp,
    double HeroXp,
    double MinionXp,
    double StructureXp,
    double TrickleXp,
    IReadOnlyCollection<ParsedTeamLevelEvent> LevelEvents,
    IReadOnlyCollection<ParsedTeamXpSnapshot> XpSnapshots);

public sealed record ParsedTeamLevelEvent(int Level, int Seconds);

public sealed record ParsedTeamXpSnapshot(
    int Seconds,
    int Level,
    double HeroXp,
    double MinionXp,
    double StructureXp,
    double TrickleXp,
    double CreepXp);

public sealed record ParsedTimelineEvent(
    string EventType,
    int Seconds,
    int? Team,
    string? UnitName,
    int? Level,
    bool? SoloDeath);
