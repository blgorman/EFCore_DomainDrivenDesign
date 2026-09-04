using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductId)
            .IsRequired();

        builder.Property(l => l.Quantity)
            .IsRequired();

        builder.OwnsOne(l => l.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice_Amount")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPrice_Currency")
                .HasMaxLength(10)
                .IsRequired();
        });
    }
}
