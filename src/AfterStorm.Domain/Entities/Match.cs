namespace AfterStorm.Domain.Entities;

public sealed class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReplayHash { get; set; } = string.Empty;
    public string Map { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public DateTimeOffset PlayedAt { get; set; }
    public int DurationSeconds { get; set; }
    public int GameBuild { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<MatchPlayer> Players { get; set; } = new List<MatchPlayer>();
}
