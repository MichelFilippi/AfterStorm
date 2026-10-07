using AfterStorm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfterStorm.Infrastructure.Persistence;

public sealed class AfterStormDbContext(DbContextOptions<AfterStormDbContext> options) : DbContext(options)
{
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<TalentSelection> TalentSelections => Set<TalentSelection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ReplayHash).IsUnique();
            entity.Property(x => x.ReplayHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Map).HasMaxLength(120).IsRequired();
            entity.Property(x => x.GameMode).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<MatchPlayer>(entity =>
        {
            entity.ToTable("match_players");
            entity.HasKey(x => x.Id);
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
    }
}
