namespace BusinessTwin.Server.Modules.Purchasing;

using BusinessTwin.Server.Modules.Inventory;

public sealed class PurchasingService
{
    private readonly Dictionary<Guid, PurchaseRequest> requests = new();
    private readonly Dictionary<Guid, PurchaseOrder> orders = new();
    private readonly Dictionary<(Guid OrderId, Guid ProductId), int> receivedQuantities = new();

    public PurchaseRequest CreateRequest(CreatePurchaseRequest request, DateTimeOffset createdAt, string createdBy)
    {
        var lines = request.Lines.Select(line => new PurchaseLine(line.ProductId, line.Quantity, line.UnitCost)).ToArray();
        var purchaseRequest = new PurchaseRequest(Guid.NewGuid(), request.SupplierId, lines, createdAt, createdBy);
        requests.Add(purchaseRequest.Id, purchaseRequest);
        return purchaseRequest;
    }

    public PurchaseRequest ChangeRequestStatus(Guid requestId, PurchaseRequestStatus status)
    {
        if (!requests.TryGetValue(requestId, out var request))
        {
            throw new KeyNotFoundException("Purchase request was not found.");
        }

        request.ChangeStatus(status);
        return request;
    }

    public PurchaseOrder CreateOrderFromApprovedRequest(Guid requestId, DateTimeOffset createdAt, string createdBy)
    {
        if (!requests.TryGetValue(requestId, out var request))
        {
            throw new KeyNotFoundException("Purchase request was not found.");
        }

        if (request.Status != PurchaseRequestStatus.Approved)
        {
            throw new InvalidOperationException("Only an approved purchase request can create an order.");
        }

        var order = new PurchaseOrder(Guid.NewGuid(), request.SupplierId, request.Lines, createdAt, createdBy);
        orders.Add(order.Id, order);
        return order;
    }

    public PurchaseOrder GetOrder(Guid orderId)
    {
        if (!orders.TryGetValue(orderId, out var order))
        {
            throw new KeyNotFoundException("Purchase order was not found.");
        }

        return order;
    }

    public PurchaseReceipt Receive(
        Guid orderId,
        ReceivePurchaseRequest request,
        InventoryService inventoryService,
        DateTimeOffset receivedAt,
        string actor)
    {
        var order = GetOrder(orderId);
        if (order.Status is not (PurchaseOrderStatus.Ordered or PurchaseOrderStatus.PartiallyReceived))
        {
            throw new InvalidOperationException("Only an ordered purchase order can receive goods.");
        }

        var orderedQuantity = order.OrderedQuantity(request.ProductId);
        var key = (orderId, request.ProductId);
        var receivedQuantity = receivedQuantities.GetValueOrDefault(key);
        if (request.Quantity <= 0 || receivedQuantity + request.Quantity > orderedQuantity)
        {
            throw new InvalidOperationException("Received quantity exceeds the ordered quantity.");
        }

        inventoryService.ApplyTransaction(
            new InventoryTransactionRequest(
                request.ProductId,
                request.WarehouseId,
                InventoryTransactionType.In,
                request.Quantity,
                $"PO-{orderId}"),
            receivedAt,
            actor);

        var totalReceived = receivedQuantity + request.Quantity;
        receivedQuantities[key] = totalReceived;
        var allLinesReceived = order.Lines.All(line =>
            receivedQuantities.GetValueOrDefault((orderId, line.ProductId)) >= line.Quantity);
        order.ChangeStatus(allLinesReceived
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.PartiallyReceived);

        return new PurchaseReceipt(orderId, request.ProductId, request.WarehouseId, request.Quantity, receivedAt, actor);
    }
}