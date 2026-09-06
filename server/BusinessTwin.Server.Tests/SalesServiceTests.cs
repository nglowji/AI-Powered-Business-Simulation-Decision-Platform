using BusinessTwin.Server.Modules.Sales;

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
}