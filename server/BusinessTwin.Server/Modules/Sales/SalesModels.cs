using BusinessTwin.Server.Shared;

namespace BusinessTwin.Server.Modules.Sales;

public enum SalesOrderStatus
{
    Draft,
    Pending,
    Confirmed,
    Processing,
    Completed,
    Cancelled
}

public sealed record Customer(Guid Id, string Name, string Email);

public sealed record SalesOrderLineRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount);

public sealed record CreateSalesOrderRequest(
    Guid CustomerId,
    IReadOnlyList<SalesOrderLineRequest> Lines);

public sealed record CompleteDeliveryRequest(Guid WarehouseId);

public sealed record SalesOrderLine(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount)
{
    public decimal Total => Quantity * UnitPrice - Discount;
}

public sealed class SalesOrder : EntityAudit
{
    public SalesOrder(
        Guid id,
        Guid customerId,
        IReadOnlyList<SalesOrderLine> lines,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id, createdAt, createdBy)
    {
        if (lines.Count == 0)
        {
            throw new ArgumentException("A sales order must contain at least one line.", nameof(lines));
        }

        if (lines.Any(line => line.Quantity <= 0 || line.UnitPrice < 0 || line.Discount < 0 || line.Discount > line.Quantity * line.UnitPrice))
        {
            throw new ArgumentException("Sales order lines contain invalid values.", nameof(lines));
        }

        CustomerId = customerId;
        Lines = lines;
        Status = SalesOrderStatus.Draft;
    }

    public Guid CustomerId { get; }

    public IReadOnlyList<SalesOrderLine> Lines { get; }

    public SalesOrderStatus Status { get; private set; }

    public decimal Total => Lines.Sum(line => line.Total);

    public void ChangeStatus(SalesOrderStatus nextStatus)
    {
        if (!IsAllowedTransition(Status, nextStatus))
        {
            throw new InvalidOperationException($"Cannot change order status from {Status} to {nextStatus}.");
        }

        Status = nextStatus;
    }

    private static bool IsAllowedTransition(SalesOrderStatus current, SalesOrderStatus next)
    {
        return (current, next) switch
        {
            (SalesOrderStatus.Draft, SalesOrderStatus.Pending) => true,
            (SalesOrderStatus.Pending, SalesOrderStatus.Confirmed) => true,
            (SalesOrderStatus.Confirmed, SalesOrderStatus.Processing) => true,
            (SalesOrderStatus.Processing, SalesOrderStatus.Completed) => true,
            (SalesOrderStatus.Draft, SalesOrderStatus.Cancelled) => true,
            (SalesOrderStatus.Pending, SalesOrderStatus.Cancelled) => true,
            (SalesOrderStatus.Confirmed, SalesOrderStatus.Cancelled) => true,
            _ => false
        };
    }
}