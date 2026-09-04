namespace OrderManagement.Domain.Events;

public record OrderCancelledEvent(int OrderId, DateTime CancelledAt) : IDomainEvent;
