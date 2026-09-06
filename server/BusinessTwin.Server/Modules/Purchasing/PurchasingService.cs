namespace BusinessTwin.Server.Modules.Purchasing;

public sealed class PurchasingService
{
    private readonly Dictionary<Guid, PurchaseRequest> requests = new();
    private readonly Dictionary<Guid, PurchaseOrder> orders = new();

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
}