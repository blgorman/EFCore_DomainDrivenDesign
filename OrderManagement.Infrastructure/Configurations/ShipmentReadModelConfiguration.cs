using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Infrastructure.ReadModels;

namespace OrderManagement.Infrastructure.Configurations;

public class ShipmentReadModelConfiguration : IEntityTypeConfiguration<ShipmentReadModel>
{
    public void Configure(EntityTypeBuilder<ShipmentReadModel> builder)
    {
        builder.ToTable("Orders", t => t.ExcludeFromMigrations());

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.PlacedAt)
            .IsRequired();
    }
}
