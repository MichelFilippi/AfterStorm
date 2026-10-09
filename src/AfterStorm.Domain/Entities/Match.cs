namespace AfterStorm.Domain.Entities;

public sealed class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReplayHash { get; set; } = string.Empty;
    public string ExternalReplayId { get; set; } = string.Empty;
    public string Map { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public DateTimeOffset PlayedAt { get; set; }
    public int DurationSeconds { get; set; }
    public int GameBuild { get; set; }
    public int ProtocolBuild { get; set; }
    public bool ProtocolFallback { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<MatchPlayer> Players { get; set; } = new List<MatchPlayer>();
    public ICollection<MatchTeam> Teams { get; set; } = new List<MatchTeam>();
    public ICollection<MatchTimelineEvent> TimelineEvents { get; set; } = new List<MatchTimelineEvent>();
}
