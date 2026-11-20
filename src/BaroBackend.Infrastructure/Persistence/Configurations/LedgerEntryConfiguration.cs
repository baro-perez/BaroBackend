using BaroBackend.Domain.Metagame;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaroBackend.Infrastructure.Persistence.Configurations;

internal sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ItemId).HasMaxLength(64);
        builder.Property(e => e.Reason).HasMaxLength(64);
        builder.HasOne<Player>().WithMany()
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.PlayerId, e.CreatedAt });

        // Immutability for every property
        builder.Property(e => e.Id).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.PlayerId).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.ItemType).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.ItemId).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.CreatedAt).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.BalanceAfter).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.Delta).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(e => e.Reason).Metadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    }
}