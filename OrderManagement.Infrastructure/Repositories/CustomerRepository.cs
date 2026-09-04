using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly OrderingContext _ctx;

    public CustomerRepository(OrderingContext ctx) => _ctx = ctx;

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public void Add(Customer customer) => _ctx.Customers.Add(customer);

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }
}
