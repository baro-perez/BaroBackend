using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaroBackend.Domain.MetaGame;

namespace BaroBackend.Infrastructure.Persistence.Configurations;

internal sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ResourceId).HasMaxLength(64);
        builder.Property(e => e.Reason).HasMaxLength(64);
        builder.HasOne<Player>().WithMany()
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.PlayerId, e.CreatedAt });
    }
}