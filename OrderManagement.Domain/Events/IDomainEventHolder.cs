namespace OrderManagement.Domain.Events;

public interface IDomainEventHolder
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearEvents();
}
