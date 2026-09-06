namespace BusinessTwin.Server.Modules.Sales;

using BusinessTwin.Server.Modules.Inventory;

public sealed class SalesService
{
    private readonly Dictionary<Guid, SalesOrder> orders = new();

    public SalesOrder CreateOrder(CreateSalesOrderRequest request, DateTimeOffset createdAt, string createdBy)
    {
        var lines = request.Lines
            .Select(line => new SalesOrderLine(line.ProductId, line.Quantity, line.UnitPrice, line.Discount))
            .ToArray();
        var order = new SalesOrder(Guid.NewGuid(), request.CustomerId, lines, createdAt, createdBy);
        orders.Add(order.Id, order);
        return order;
    }

    public SalesOrder GetOrder(Guid orderId)
    {
        if (!orders.TryGetValue(orderId, out var order))
        {
            throw new KeyNotFoundException("Sales order was not found.");
        }

        return order;
    }

    public SalesOrder ChangeStatus(Guid orderId, SalesOrderStatus status)
    {
        var order = GetOrder(orderId);
        order.ChangeStatus(status);
        return order;
    }

    public IReadOnlyList<InventoryTransaction> CompleteDelivery(
        Guid orderId,
        CompleteDeliveryRequest request,
        InventoryService inventoryService,
        DateTimeOffset deliveredAt,
        string actor)
    {
        var order = GetOrder(orderId);
        if (order.Status != SalesOrderStatus.Processing)
        {
            throw new InvalidOperationException("Only a processing order can be delivered.");
        }

        foreach (var line in order.Lines)
        {
            var balance = inventoryService.GetBalance(line.ProductId, request.WarehouseId);
            if (balance.AvailableQuantity < line.Quantity)
            {
                throw new InvalidOperationException("The order exceeds available inventory.");
            }
        }

        var transactions = order.Lines.Select(line => inventoryService.ApplyTransaction(
            new InventoryTransactionRequest(
                line.ProductId,
                request.WarehouseId,
                InventoryTransactionType.Out,
                line.Quantity,
                $"SO-{orderId}"),
            deliveredAt,
            actor)).ToArray();
        order.ChangeStatus(SalesOrderStatus.Completed);
        return transactions;
    }
}