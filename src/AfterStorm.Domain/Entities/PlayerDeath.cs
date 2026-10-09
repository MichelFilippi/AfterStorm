namespace AfterStorm.Domain.Entities;

public sealed class PlayerDeath
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchPlayerId { get; set; }
    public MatchPlayer MatchPlayer { get; set; } = null!;

    public int Seconds { get; set; }
    public bool SoloDeath { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public string KillersJson { get; set; } = "[]";
}
