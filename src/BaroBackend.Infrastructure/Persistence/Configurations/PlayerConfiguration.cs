using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaroBackend.Domain.Metagame;

namespace BaroBackend.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public const string PlayerIdKey = "PlayerId";

    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .HasColumnType(PostgresConstants.CaseInsensitiveText)
            .IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();

        builder.OwnsMany(p => p.Balances, resourceBalanceBuilder =>
        {
            resourceBalanceBuilder.WithOwner().HasForeignKey(PlayerIdKey);
            resourceBalanceBuilder.HasKey(PlayerIdKey, nameof(ResourceAndAmount.ResourceId));
            resourceBalanceBuilder.Property(b => b.ResourceId).HasMaxLength(64);
            resourceBalanceBuilder.ToTable("resource_balances", t =>
            {
                t.HasCheckConstraint("ck_resource_balances_amount_non_negative", 
                    "amount >= 0");
            });
        });
        builder.Navigation(p => p.Balances).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.ToTable("players", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_players_name_length", "char_length(name) between 3 and 32");
        });
    }
}
