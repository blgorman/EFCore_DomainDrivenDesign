using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Outbox;

// Module 7 demo context. The OutboxMessages table is not part of the Ordering
// migrations — DemonstrateOutboxPattern creates it at runtime, so the schema of
// every other module's demo run stays untouched.
public class OutboxOrderingContext : OrderingContext
{
    public OutboxOrderingContext(DbContextOptions<OrderingContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
