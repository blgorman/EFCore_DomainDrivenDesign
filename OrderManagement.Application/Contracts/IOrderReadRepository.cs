namespace OrderManagement.Application.Contracts;

public interface IOrderReadRepository
{
    Task<IReadOnlyList<OrderSummaryDto>> ListSummariesAsync(CancellationToken ct = default);
}
