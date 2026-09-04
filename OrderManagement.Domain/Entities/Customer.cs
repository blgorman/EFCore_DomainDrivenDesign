using OrderManagement.Domain.Events;

namespace OrderManagement.Domain.Entities;

public class Customer : AggregateRoot
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    protected Customer() { }

    public static Customer Create(string name, string email, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email cannot be empty.", nameof(email));

        return new Customer
        {
            Id = id ?? 0,
            Name = name,
            Email = email
        };
    }
}
