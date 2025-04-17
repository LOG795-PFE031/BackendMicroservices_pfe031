using Microsoft.EntityFrameworkCore;
using Portfolio.Commands.Interfaces;
using Portfolio.Domain;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Repositories;

public sealed class WalletContext : DbContext, IMigrateWalletContext
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>(builder =>
        {
            builder.HasKey(w => w.Id);
            builder.Ignore(w => w.DomainEvents);
            builder.OwnsOne(w => w.Balance, b =>
            {
                b.Property(m => m.Value).HasColumnName("Balance");
            });

            builder.OwnsMany(w => w.Shares, s =>
            {
                s.ToTable("WalletShares");
                s.WithOwner().HasForeignKey("WalletId");
                s.HasKey("WalletId", nameof(ShareVolume.Symbol));
                s.Property(v => v.Symbol).HasColumnName("Symbol");
                s.Property(v => v.Volume).HasColumnName("Volume");
            });
        });
    }

    public void Migrate()
    {
        Database.Migrate();
    }
}