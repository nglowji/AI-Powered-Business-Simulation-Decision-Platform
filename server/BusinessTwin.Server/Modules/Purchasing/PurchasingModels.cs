using BusinessTwin.Server.Shared;

namespace BusinessTwin.Server.Modules.Purchasing;

public enum PurchaseRequestStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    Cancelled
}

public enum PurchaseOrderStatus
{
    Draft,
    Ordered,
    PartiallyReceived,
    Received,
    Cancelled
}

public enum PaymentStatus
{
    Unpaid,
    PartiallyPaid,
    Paid
}

public sealed record Supplier(Guid Id, string Name, string Email);

public sealed record PurchaseLineRequest(Guid ProductId, int Quantity, decimal UnitCost);

public sealed record CreatePurchaseRequest(Guid SupplierId, IReadOnlyList<PurchaseLineRequest> Lines);

public sealed record PurchaseLine(Guid ProductId, int Quantity, decimal UnitCost)
{
    public decimal Total => Quantity * UnitCost;
}

public sealed class PurchaseRequest : EntityAudit
{
    public PurchaseRequest(
        Guid id,
        Guid supplierId,
        IReadOnlyList<PurchaseLine> lines,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id, createdAt, createdBy)
    {
        if (lines.Count == 0 || lines.Any(line => line.Quantity <= 0 || line.UnitCost < 0))
        {
            throw new ArgumentException("A purchase request must contain valid lines.", nameof(lines));
        }

        SupplierId = supplierId;
        Lines = lines;
        Status = PurchaseRequestStatus.Draft;
    }

    public Guid SupplierId { get; }

    public IReadOnlyList<PurchaseLine> Lines { get; }

    public PurchaseRequestStatus Status { get; private set; }

    public decimal Total => Lines.Sum(line => line.Total);

    public void ChangeStatus(PurchaseRequestStatus nextStatus)
    {
        var allowed = (Status, nextStatus) switch
        {
            (PurchaseRequestStatus.Draft, PurchaseRequestStatus.PendingApproval) => true,
            (PurchaseRequestStatus.PendingApproval, PurchaseRequestStatus.Approved) => true,
            (PurchaseRequestStatus.PendingApproval, PurchaseRequestStatus.Rejected) => true,
            (PurchaseRequestStatus.Draft, PurchaseRequestStatus.Cancelled) => true,
            (PurchaseRequestStatus.PendingApproval, PurchaseRequestStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException($"Cannot change purchase request status from {Status} to {nextStatus}.");
        }

        Status = nextStatus;
    }
}

public sealed class PurchaseOrder : EntityAudit
{
    public PurchaseOrder(
        Guid id,
        Guid supplierId,
        IReadOnlyList<PurchaseLine> lines,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id, createdAt, createdBy)
    {
        SupplierId = supplierId;
        Lines = lines;
        Status = PurchaseOrderStatus.Draft;
        PaymentStatus = PaymentStatus.Unpaid;
    }

    public Guid SupplierId { get; }

    public IReadOnlyList<PurchaseLine> Lines { get; }

    public PurchaseOrderStatus Status { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public decimal Total => Lines.Sum(line => line.Total);

    public void ChangeStatus(PurchaseOrderStatus nextStatus)
    {
        var allowed = (Status, nextStatus) switch
        {
            (PurchaseOrderStatus.Draft, PurchaseOrderStatus.Ordered) => true,
            (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.PartiallyReceived) => true,
            (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.Received) => true,
            (PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.Received) => true,
            (PurchaseOrderStatus.Draft, PurchaseOrderStatus.Cancelled) => true,
            (PurchaseOrderStatus.Ordered, PurchaseOrderStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException($"Cannot change purchase order status from {Status} to {nextStatus}.");
        }

        Status = nextStatus;
    }

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0 || PaymentStatus == PaymentStatus.Paid)
        {
            throw new InvalidOperationException("Payment amount or payment state is invalid.");
        }

        PaymentStatus = amount >= Total ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
    }
}