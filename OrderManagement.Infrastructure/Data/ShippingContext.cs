using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Configurations;
using OrderManagement.Infrastructure.ReadModels;

namespace OrderManagement.Infrastructure.Data;

public class ShippingContext : DbContext
{
    //TODO: Module 3 Clip 4 — Add a DbSet<ShipmentReadModel> so ShippingContext can query its lean projection.
    //public DbSet<ShipmentReadModel> Shipments => Set<ShipmentReadModel>();

    public ShippingContext(DbContextOptions<ShippingContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //TODO: Module 3 Clip 4 — Register ShipmentReadModelConfiguration so EF maps ShipmentReadModel to the Orders table.
        //modelBuilder.ApplyConfiguration(new ShipmentReadModelConfiguration());
    }
}
