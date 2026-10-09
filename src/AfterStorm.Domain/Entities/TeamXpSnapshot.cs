namespace AfterStorm.Domain.Entities;

public sealed class TeamXpSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchTeamId { get; set; }
    public MatchTeam MatchTeam { get; set; } = null!;

    public int Seconds { get; set; }
    public int Level { get; set; }
    public double HeroXp { get; set; }
    public double MinionXp { get; set; }
    public double StructureXp { get; set; }
    public double TrickleXp { get; set; }
    public double CreepXp { get; set; }
}
