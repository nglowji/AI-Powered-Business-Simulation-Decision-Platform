namespace BusinessTwin.Server.Modules.Inventory;

public sealed class InventoryService
{
    private readonly Dictionary<(Guid ProductId, Guid WarehouseId), InventoryItem> items = new();
    private readonly List<InventoryTransaction> transactions = [];

    public InventoryItem CreateBalance(
        Guid productId,
        Guid warehouseId,
        int quantity,
        DateTimeOffset createdAt,
        string createdBy)
    {
        var key = (productId, warehouseId);
        if (items.ContainsKey(key))
        {
            throw new InvalidOperationException("An inventory balance already exists for this product and warehouse.");
        }

        var item = new InventoryItem(Guid.NewGuid(), productId, warehouseId, quantity, 0, createdAt, createdBy);
        items.Add(key, item);
        return item;
    }

    public InventoryBalanceResponse GetBalance(Guid productId, Guid warehouseId)
    {
        var item = GetItem(productId, warehouseId);
        return new InventoryBalanceResponse(
            item.ProductId,
            item.WarehouseId,
            item.Quantity,
            item.ReservedQuantity,
            item.AvailableQuantity);
    }

    public InventoryTransaction ApplyTransaction(
        InventoryTransactionRequest request,
        DateTimeOffset occurredAt,
        string actor)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Quantity), "Transaction quantity must be greater than zero.");
        }

        var item = GetItem(request.ProductId, request.WarehouseId);
        var quantityDelta = request.Type switch
        {
            InventoryTransactionType.In => request.Quantity,
            InventoryTransactionType.Return => request.Quantity,
            InventoryTransactionType.Out => -request.Quantity,
            InventoryTransactionType.Adjustment => request.Quantity,
            InventoryTransactionType.Transfer => -request.Quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(request.Type))
        };

        if (quantityDelta < 0 && request.Quantity > item.AvailableQuantity)
        {
            throw new InvalidOperationException("The transaction exceeds available inventory.");
        }

        item.Apply(quantityDelta);
        var transaction = new InventoryTransaction(
            Guid.NewGuid(),
            request.ProductId,
            request.WarehouseId,
            request.Type,
            request.Quantity,
            request.SourceReference,
            occurredAt,
            actor);
        transactions.Add(transaction);
        return transaction;
    }

    public void Reserve(Guid productId, Guid warehouseId, int quantity)
    {
        GetItem(productId, warehouseId).Reserve(quantity);
    }

    public IReadOnlyList<InventoryTransaction> GetTransactions() => transactions;

    private InventoryItem GetItem(Guid productId, Guid warehouseId)
    {
        if (!items.TryGetValue((productId, warehouseId), out var item))
        {
            throw new KeyNotFoundException("Inventory balance was not found.");
        }

        return item;
    }
}