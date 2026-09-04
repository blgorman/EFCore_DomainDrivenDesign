namespace AppModels;

/// <summary>
/// Anemic model used in Module 1 Clip 2 to demonstrate the "before" state.
/// All properties are public setters — no invariants, no guard clauses.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
