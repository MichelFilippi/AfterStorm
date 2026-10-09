namespace AfterStorm.Domain.Entities;

public sealed class TeamLevelEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchTeamId { get; set; }
    public MatchTeam MatchTeam { get; set; } = null!;
    public int Level { get; set; }
    public int Seconds { get; set; }
}
