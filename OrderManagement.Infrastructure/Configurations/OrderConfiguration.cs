using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities; //do not remove, needed for the Order Customer reference

namespace OrderManagement.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.PlacedAt)
            .IsRequired();

        //TODO: Module 2 Clip 4 — After changing Order.Total from decimal to Money, replace this line
        builder.Property(o => o.Total)
            .IsRequired();
        //  with:
        //builder.OwnsOne(o => o.Total, money =>
        //{
        //    money.Property(m => m.Amount).HasColumnName("Total").IsRequired();
        //    money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        //});

        //TODO: Module 2 Clip 5 — After removing the CLR CustomerId property, replace this line
        builder.Property(o => o.CustomerId)
            .IsRequired();
        //  with:
        //builder.Property<int>("CustomerId");
        //builder.HasOne<Customer>()
        //    .WithMany()
        //    .HasForeignKey("CustomerId")
        //    .IsRequired();

        builder.Navigation(o => o.Lines)
            .HasField("_lines");

        // OrderId is a shadow FK from day one — OrderLine has no CLR OrderId property.
        // This is the reference example for Clip 5: the same pattern applied to Order.CustomerId.
        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired();
    }
}
