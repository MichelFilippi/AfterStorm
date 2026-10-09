namespace AfterStorm.Domain.Entities;

public sealed class MatchTeam
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int Team { get; set; }
    public string TeamId { get; set; } = string.Empty;
    public bool IsWinner { get; set; }
    public int Level { get; set; }
    public double TotalXp { get; set; }
    public double HeroXp { get; set; }
    public double MinionXp { get; set; }
    public double StructureXp { get; set; }
    public double TrickleXp { get; set; }

    public ICollection<TeamLevelEvent> LevelEvents { get; set; } = new List<TeamLevelEvent>();
    public ICollection<TeamXpSnapshot> XpSnapshots { get; set; } = new List<TeamXpSnapshot>();
}
