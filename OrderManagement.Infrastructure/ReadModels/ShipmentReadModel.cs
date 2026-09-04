using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.ReadModels;

public class ShipmentReadModel
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime PlacedAt { get; set; }
}
