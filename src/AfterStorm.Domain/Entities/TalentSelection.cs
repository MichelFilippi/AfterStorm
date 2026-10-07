namespace AfterStorm.Domain.Entities;

public sealed class TalentSelection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchPlayerId { get; set; }
    public MatchPlayer MatchPlayer { get; set; } = null!;

    public int Tier { get; set; }
    public string InternalName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int GameLoop { get; set; }
    public int SelectedAtSeconds { get; set; }
}
