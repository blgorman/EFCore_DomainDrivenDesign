namespace OrderManagement.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        //TODO: Module 2 Clip 8 — Add guard clauses:
        //if (amount < 0)
        //    throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        //if (string.IsNullOrWhiteSpace(currency))
        //    throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        //if (currency.Length > 10)
        //    throw new ArgumentException("Currency code must be 10 characters or fewer.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        //TODO: Module 2 Clip 8 — Add guard clause:
        //if (other.Currency != Currency)
        //    throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}: currency mismatch.");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        //TODO: Module 2 Clip 8 — Add guard clause:
        //if (quantity < 0)
        //    throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        return new Money(Amount * quantity, Currency);
    }
}
