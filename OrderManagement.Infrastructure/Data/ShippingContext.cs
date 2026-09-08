using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Configurations;
using OrderManagement.Infrastructure.ReadModels;

namespace OrderManagement.Infrastructure.Data;

public class ShippingContext : DbContext
{
    public DbSet<ShipmentReadModel> Shipments => Set<ShipmentReadModel>();

    public ShippingContext(DbContextOptions<ShippingContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ShipmentReadModelConfiguration());
    }
}
