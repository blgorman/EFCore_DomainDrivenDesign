using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Configurations;

namespace OrderManagement.Infrastructure.Data;

public class OrderingContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    public OrderingContext(DbContextOptions<OrderingContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
