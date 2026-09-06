using BusinessTwin.Server.Shared;

namespace BusinessTwin.Server.Modules.Inventory;

public enum InventoryTransactionType
{
    In,
    Out,
    Transfer,
    Adjustment,
    Return
}

public sealed record Product(Guid Id, string Sku, string Name);

public sealed record Warehouse(Guid Id, string Code, string Name);

public sealed class InventoryItem : EntityAudit
{
    public InventoryItem(
        Guid id,
        Guid productId,
        Guid warehouseId,
        int quantity,
        int reservedQuantity,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id, createdAt, createdBy)
    {
        if (quantity < 0 || reservedQuantity < 0 || reservedQuantity > quantity)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Inventory quantities are invalid.");
        }

        ProductId = productId;
        WarehouseId = warehouseId;
        Quantity = quantity;
        ReservedQuantity = reservedQuantity;
    }

    public Guid ProductId { get; }

    public Guid WarehouseId { get; }

    public int Quantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    public void Apply(int quantityDelta)
    {
        var nextQuantity = Quantity + quantityDelta;
        if (nextQuantity < ReservedQuantity)
        {
            throw new InvalidOperationException("Inventory cannot fall below the reserved quantity.");
        }

        Quantity = nextQuantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0 || quantity > AvailableQuantity)
        {
            throw new InvalidOperationException("The requested quantity is not available for reservation.");
        }

        ReservedQuantity += quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity)
        {
            throw new InvalidOperationException("The requested quantity is not reserved.");
        }

        ReservedQuantity -= quantity;
    }
}

public sealed record InventoryTransaction(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    string SourceReference,
    DateTimeOffset OccurredAt,
    string Actor);

public sealed record InventoryBalanceResponse(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity);

public sealed record InventoryTransactionRequest(
    Guid ProductId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    string SourceReference);