namespace AfterStorm.Domain.Entities;

public sealed class MatchPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int ReplayPlayerId { get; set; }
    public string BattleTag { get; set; } = string.Empty;
    public string ToonHandle { get; set; } = string.Empty;
    public string Hero { get; set; } = string.Empty;
    public int Team { get; set; }
    public bool Won { get; set; }
    public int? Kills { get; set; }
    public int? Deaths { get; set; }
    public int? Assists { get; set; }

    public ICollection<TalentSelection> Talents { get; set; } = new List<TalentSelection>();
}
