using BusinessTwin.Server.Modules.Sales;
using BusinessTwin.Server.Modules.Inventory;

namespace BusinessTwin.Server.Tests;

public sealed class SalesServiceTests
{
    [Fact]
    public void CreateOrder_CalculatesLineTotalsAndStartsAsDraft()
    {
        var service = new SalesService();
        var order = service.CreateOrder(
            new CreateSalesOrderRequest(
                Guid.NewGuid(),
                [new SalesOrderLineRequest(Guid.NewGuid(), 2, 100m, 10m)]),
            DateTimeOffset.UtcNow,
            "tester");

        Assert.Equal(SalesOrderStatus.Draft, order.Status);
        Assert.Equal(190m, order.Total);
    }

    [Fact]
    public void ChangeStatus_RejectsSkippingWorkflowStates()
    {
        var service = new SalesService();
        var order = service.CreateOrder(
            new CreateSalesOrderRequest(
                Guid.NewGuid(),
                [new SalesOrderLineRequest(Guid.NewGuid(), 1, 20m, 0m)]),
            DateTimeOffset.UtcNow,
            "tester");

        var action = () => service.ChangeStatus(order.Id, SalesOrderStatus.Completed);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void ChangeStatus_AllowsNormalOrderWorkflow()
    {
        var service = new SalesService();
        var order = service.CreateOrder(
            new CreateSalesOrderRequest(
                Guid.NewGuid(),
                [new SalesOrderLineRequest(Guid.NewGuid(), 1, 20m, 0m)]),
            DateTimeOffset.UtcNow,
            "tester");

        service.ChangeStatus(order.Id, SalesOrderStatus.Pending);
        service.ChangeStatus(order.Id, SalesOrderStatus.Confirmed);
        service.ChangeStatus(order.Id, SalesOrderStatus.Processing);
        service.ChangeStatus(order.Id, SalesOrderStatus.Completed);

        Assert.Equal(SalesOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void CompleteDelivery_UpdatesInventoryAndCompletesProcessingOrder()
    {
        var service = new SalesService();
        var inventory = new InventoryService();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        inventory.CreateBalance(productId, warehouseId, 10, DateTimeOffset.UtcNow, "seed");
        var order = service.CreateOrder(
            new CreateSalesOrderRequest(Guid.NewGuid(), [new SalesOrderLineRequest(productId, 3, 20m, 0m)]),
            DateTimeOffset.UtcNow,
            "tester");
        service.ChangeStatus(order.Id, SalesOrderStatus.Pending);
        service.ChangeStatus(order.Id, SalesOrderStatus.Confirmed);
        service.ChangeStatus(order.Id, SalesOrderStatus.Processing);

        var transactions = service.CompleteDelivery(
            order.Id,
            new CompleteDeliveryRequest(warehouseId),
            inventory,
            DateTimeOffset.UtcNow,
            "tester");

        Assert.Single(transactions);
        Assert.Equal(SalesOrderStatus.Completed, order.Status);
        Assert.Equal(7, inventory.GetBalance(productId, warehouseId).Quantity);
    }

    [Fact]
    public void CompleteDelivery_WhenOneLineLacksStock_DoesNotWriteTransactions()
    {
        var service = new SalesService();
        var inventory = new InventoryService();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        inventory.CreateBalance(firstProductId, warehouseId, 10, DateTimeOffset.UtcNow, "seed");
        inventory.CreateBalance(secondProductId, warehouseId, 1, DateTimeOffset.UtcNow, "seed");
        var order = service.CreateOrder(
            new CreateSalesOrderRequest(Guid.NewGuid(),
            [
                new SalesOrderLineRequest(firstProductId, 3, 20m, 0m),
                new SalesOrderLineRequest(secondProductId, 2, 20m, 0m)
            ]),
            DateTimeOffset.UtcNow,
            "tester");
        service.ChangeStatus(order.Id, SalesOrderStatus.Pending);
        service.ChangeStatus(order.Id, SalesOrderStatus.Confirmed);
        service.ChangeStatus(order.Id, SalesOrderStatus.Processing);

        Assert.Throws<InvalidOperationException>(() => service.CompleteDelivery(
            order.Id,
            new CompleteDeliveryRequest(warehouseId),
            inventory,
            DateTimeOffset.UtcNow,
            "tester"));
        Assert.Equal(10, inventory.GetBalance(firstProductId, warehouseId).Quantity);
        Assert.Empty(inventory.GetTransactions());
    }
}