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

    // Kills is kept for backwards compatibility. For HOTS, prefer Takedowns/SoloKills.
    public int? Kills { get; set; }
    public int? Takedowns { get; set; }
    public int? SoloKills { get; set; }
    public int? Deaths { get; set; }
    public int? Assists { get; set; }
    public int? HeroDamage { get; set; }
    public int? SiegeDamage { get; set; }
    public int? DamageTaken { get; set; }
    public int? Healing { get; set; }
    public int? SelfHealing { get; set; }
    public int? ExperienceContribution { get; set; }
    public int? TimeSpentDead { get; set; }
    public int? RegenGlobes { get; set; }

    public ICollection<TalentSelection> Talents { get; set; } = new List<TalentSelection>();
    public ICollection<PlayerDeath> DeathEvents { get; set; } = new List<PlayerDeath>();
}
