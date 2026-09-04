using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain;

[Trait("Category", "Unit")]
[Trait("Module", "2")]
public class MoneyTests
{
    [Fact]
    [Trait("Clip", "4")]
    public void Create_WithValidValues_Succeeds()
    {
        var money = Money.Create(9.99m, "USD");

        money.Amount.ShouldBe(9.99m);
        money.Currency.ShouldBe("USD");
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Create_WithNegativeAmount_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Money.Create(-1m, "USD"));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Create_WithEmptyCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Money.Create(10m, ""));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Create_WithWhitespaceCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Money.Create(10m, "   "));
    }

    [Fact]
    [Trait("Clip", "4")]
    public void SameValues_AreEqual()
    {
        var a = Money.Create(10m, "USD");
        var b = Money.Create(10m, "USD");

        b.ShouldBe(a);
    }

    [Fact]
    [Trait("Clip", "4")]
    public void DifferentCurrency_NotEqual()
    {
        var a = Money.Create(10m, "USD");
        var b = Money.Create(10m, "EUR");

        b.ShouldNotBe(a);
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Create_WithNullCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Money.Create(10m, null!));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Create_WithCurrencyTooLong_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Money.Create(10m, "TOOLONGCURRENCY"));
    }

    [Fact]
    [Trait("Clip", "4")]
    public void Add_SameCurrency_ReturnsSummedAmount()
    {
        var a = Money.Create(10m, "USD");
        var b = Money.Create(5m, "USD");

        var result = a.Add(b);

        result.Amount.ShouldBe(15m);
        result.Currency.ShouldBe("USD");
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Add_MismatchedCurrency_ThrowsInvalidOperationException()
    {
        var usd = Money.Create(10m, "USD");
        var eur = Money.Create(10m, "EUR");

        Should.Throw<InvalidOperationException>(() => usd.Add(eur));
    }

    [Fact]
    [Trait("Clip", "4")]
    public void Multiply_PositiveQuantity_ReturnsScaledAmount()
    {
        var money = Money.Create(10m, "USD");

        var result = money.Multiply(3);

        result.Amount.ShouldBe(30m);
        result.Currency.ShouldBe("USD");
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Multiply_NegativeQuantity_ThrowsArgumentException()
    {
        var money = Money.Create(10m, "USD");

        Should.Throw<ArgumentException>(() => money.Multiply(-1));
    }
}
