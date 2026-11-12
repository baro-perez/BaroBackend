using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaroBackend.Domain.Metagame;

namespace BaroBackend.Infrastructure.Persistence.Configurations;

internal sealed class ResourceBalanceConfiguration : IEntityTypeConfiguration<ResourceBalance>
{
    public void Configure(EntityTypeBuilder<ResourceBalance> builder)
    {
        builder.ToTable("resource_balances");
        builder.HasKey(b => new { b.PlayerId, b.ResourceId });
        builder.Property(b => b.ResourceId).HasMaxLength(64);
        builder.ToTable("resource_balances", t =>
        {
            t.HasCheckConstraint("ck_resource_balances_amount_non_negative", "amount >= 0");
        });
    }
}
