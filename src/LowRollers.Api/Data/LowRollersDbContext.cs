using LowRollers.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LowRollers.Api.Data;

public class LowRollersDbContext(DbContextOptions<LowRollersDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<GameTable> GameTables => Set<GameTable>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameSessionPlayer> GameSessionPlayers => Set<GameSessionPlayer>();
    public DbSet<SessionTransaction> SessionTransactions => Set<SessionTransaction>();
    public DbSet<SessionHand> SessionHands => Set<SessionHand>();
    public DbSet<SessionHandEvent> SessionHandEvents => Set<SessionHandEvent>();
    public DbSet<TableTemplate> TableTemplates => Set<TableTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureGame(modelBuilder);
        ConfigurePlayer(modelBuilder);
        ConfigureGameTable(modelBuilder);
        ConfigureGameSession(modelBuilder);
        ConfigureGameSessionPlayer(modelBuilder);
        ConfigureSessionTransaction(modelBuilder);
        ConfigureSessionHand(modelBuilder);
        ConfigureSessionHandEvent(modelBuilder);
        ConfigureTableTemplate(modelBuilder);
    }

    private static void ConfigureGame(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Games");
            entity.HasKey(e => e.GameId);
            entity.Property(e => e.GameId).ValueGeneratedNever();
            entity.Property(e => e.GameName).HasMaxLength(50).IsRequired();
        });
    }

    private static void ConfigurePlayer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(e => e.PlayerId);
            entity.Property(e => e.PlayerId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.PlayerName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PlayerEmail).HasMaxLength(255);
        });
    }

    private static void ConfigureGameTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameTable>(entity =>
        {
            entity.ToTable("GameTables");
            entity.HasKey(e => e.TableId);
            entity.Property(e => e.TableId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.ModifiedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TableConfig).IsRequired();

            entity.HasOne(e => e.Game)
                .WithMany(g => g.GameTables)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByPlayer)
                .WithMany(p => p.CreatedTables)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ModifiedByPlayer)
                .WithMany(p => p.ModifiedTables)
                .HasForeignKey(e => e.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                .WithMany(p => p.OwnedTables)
                .HasForeignKey(e => e.TableOwner)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGameSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.ToTable("GameSessions");
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.SessionId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.StartedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.InviteCodeHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(e => e.GameTable)
                .WithMany(t => t.GameSessions)
                .HasForeignKey(e => e.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TableId);
            entity.HasIndex(e => e.InviteCodeHash);
        });
    }

    private static void ConfigureGameSessionPlayer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSessionPlayer>(entity =>
        {
            entity.ToTable("GameSessionPlayers");
            entity.HasKey(e => new { e.SessionId, e.PlayerId });
            entity.Property(e => e.SeatedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.TimeBankSeconds).HasDefaultValue(0);
            entity.Property(e => e.ChipStack).HasDefaultValue(0);
            entity.Property(e => e.IsHost).HasDefaultValue(false);

            entity.HasOne(e => e.GameSession)
                .WithMany(s => s.GameSessionPlayers)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Player)
                .WithMany(p => p.GameSessionPlayers)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PlayerId);
            entity.HasIndex(e => new { e.SessionId, e.SeatNumber })
                .IsUnique()
                .HasFilter("[SeatNumber] > 0");

            entity.ToTable(t => t.HasCheckConstraint("CHK_GameSessionPlayers_SeatNumber", "[SeatNumber] BETWEEN 0 AND 10"));
        });
    }

    private static void ConfigureSessionTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionTransaction>(entity =>
        {
            entity.ToTable("SessionTransactions");
            entity.HasKey(e => e.SessionTransactionId);
            entity.Property(e => e.SessionTransactionId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasOne(e => e.GameSession)
                .WithMany(s => s.SessionTransactions)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Player)
                .WithMany(p => p.SessionTransactions)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.PlayerId);

            entity.ToTable(t => t.HasCheckConstraint("CHK_SessionTransactions_Amount", "[Amount] >= 0"));
        });
    }

    private static void ConfigureSessionHand(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionHand>(entity =>
        {
            entity.ToTable("SessionHands");
            entity.HasKey(e => e.HandId);
            entity.Property(e => e.HandId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.StartedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.ShuffleSeed).HasMaxLength(255).IsRequired();

            entity.HasOne(e => e.GameSession)
                .WithMany(s => s.SessionHands)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SessionId, e.HandId }).IsUnique();
        });
    }

    private static void ConfigureSessionHandEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionHandEvent>(entity =>
        {
            entity.ToTable("SessionHandEvents");
            entity.HasKey(e => e.HandDetailId);
            entity.Property(e => e.HandDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.EventTimestamp).HasDefaultValueSql("SYSDATETIMEOFFSET()");

            entity.HasOne(e => e.SessionHand)
                .WithMany(h => h.SessionHandEvents)
                .HasForeignKey(e => new { e.SessionId, e.HandId })
                .HasPrincipalKey(h => new { h.SessionId, h.HandId })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Player)
                .WithMany(p => p.SessionHandEvents)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.HandId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.PlayerId).HasFilter("[PlayerId] IS NOT NULL");
        });
    }

    private static void ConfigureTableTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableTemplate>(entity =>
        {
            entity.ToTable("TableTemplates");
            entity.HasKey(e => e.TemplateId);
            entity.Property(e => e.TemplateId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConfigJson).IsRequired();

            entity.HasOne(e => e.Owner)
                .WithMany(p => p.TableTemplates)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OwnerId);
        });
    }
}
