using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Aggregates;

public class OrderLine
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderLine()
    {
        UnitPrice = null!;
    }

    internal OrderLine(int productId, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        ArgumentNullException.ThrowIfNull(unitPrice);

        Id = 0;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
