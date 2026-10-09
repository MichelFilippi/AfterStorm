using AfterStorm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfterStorm.Infrastructure.Persistence;

public sealed class AfterStormDbContext(DbContextOptions<AfterStormDbContext> options) : DbContext(options)
{
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<TalentSelection> TalentSelections => Set<TalentSelection>();
    public DbSet<PlayerDeath> PlayerDeaths => Set<PlayerDeath>();
    public DbSet<MatchTimelineEvent> MatchTimelineEvents => Set<MatchTimelineEvent>();
    public DbSet<MatchTeam> MatchTeams => Set<MatchTeam>();
    public DbSet<TeamLevelEvent> TeamLevelEvents => Set<TeamLevelEvent>();
    public DbSet<TeamXpSnapshot> TeamXpSnapshots => Set<TeamXpSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ReplayHash).IsUnique();
            entity.HasIndex(x => x.ExternalReplayId);
            entity.Property(x => x.ReplayHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalReplayId).HasMaxLength(80);
            entity.Property(x => x.Map).HasMaxLength(120).IsRequired();
            entity.Property(x => x.GameMode).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<MatchPlayer>(entity =>
        {
            entity.ToTable("match_players");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MatchId, x.ReplayPlayerId }).IsUnique();
            entity.Property(x => x.BattleTag).HasMaxLength(80);
            entity.Property(x => x.ToonHandle).HasMaxLength(120);
            entity.Property(x => x.Hero).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Match)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TalentSelection>(entity =>
        {
            entity.ToTable("talent_selections");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InternalName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(180).IsRequired();
            entity.HasOne(x => x.MatchPlayer)
                .WithMany(x => x.Talents)
                .HasForeignKey(x => x.MatchPlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerDeath>(entity =>
        {
            entity.ToTable("player_deaths");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.KillersJson).HasColumnType("jsonb");
            entity.HasOne(x => x.MatchPlayer)
                .WithMany(x => x.DeathEvents)
                .HasForeignKey(x => x.MatchPlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchTimelineEvent>(entity =>
        {
            entity.ToTable("match_timeline_events");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MatchId, x.Seconds });
            entity.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.UnitName).HasMaxLength(120);
            entity.HasOne(x => x.Match)
                .WithMany(x => x.TimelineEvents)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchTeam>(entity =>
        {
            entity.ToTable("match_teams");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.MatchId, x.Team }).IsUnique();
            entity.Property(x => x.TeamId).HasMaxLength(30);
            entity.HasOne(x => x.Match)
                .WithMany(x => x.Teams)
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeamLevelEvent>(entity =>
        {
            entity.ToTable("team_level_events");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.MatchTeam)
                .WithMany(x => x.LevelEvents)
                .HasForeignKey(x => x.MatchTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeamXpSnapshot>(entity =>
        {
            entity.ToTable("team_xp_snapshots");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.MatchTeam)
                .WithMany(x => x.XpSnapshots)
                .HasForeignKey(x => x.MatchTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
