using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Infrastructure.ReadModels;

namespace OrderManagement.Infrastructure.Configurations;

public class ShipmentReadModelConfiguration : IEntityTypeConfiguration<ShipmentReadModel>
{
    public void Configure(EntityTypeBuilder<ShipmentReadModel> builder)
    {
        //TODO: Module 3 Clip 5 — Replace the bare ToTable with ExcludeFromMigrations so ShippingContext never
        // generates schema changes for the Orders table (owned by OrderingContext).
        //builder.ToTable("Orders", t => t.ExcludeFromMigrations());
        builder.ToTable("Orders");

        builder.HasKey(s => s.Id);

        //TODO: Module 3 Clip 5 — Add HasConversion<string>() and HasMaxLength(20) to match OrderConfiguration's
        // Status column type (nvarchar, not int).
        //builder.Property(s => s.Status)
        //    .IsRequired()
        //    .HasConversion<string>()
        //    .HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired();

        builder.Property(s => s.PlacedAt)
            .IsRequired();
    }
}
