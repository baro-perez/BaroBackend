using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaroBackend.Domain.MetaGame;

namespace BaroBackend.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(32).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();

        builder.HasMany(p => p.Balances).WithOne().HasForeignKey(b => b.PlayerId);
        builder.Navigation(p => p.Balances).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
