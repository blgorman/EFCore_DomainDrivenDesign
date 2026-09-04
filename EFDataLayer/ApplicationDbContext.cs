using Microsoft.EntityFrameworkCore;

namespace EFDataLayer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() 
    {
        //blank by design
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
