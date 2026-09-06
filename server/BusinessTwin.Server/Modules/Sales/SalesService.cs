namespace BusinessTwin.Server.Modules.Sales;

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
}