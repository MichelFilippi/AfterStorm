namespace AfterStorm.Domain.Entities;

public sealed class MatchTimelineEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;
    public int Seconds { get; set; }
    public int? Team { get; set; }
    public string? UnitName { get; set; }
    public int? Level { get; set; }
    public bool? SoloDeath { get; set; }
}
