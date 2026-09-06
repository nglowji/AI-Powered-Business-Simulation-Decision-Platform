using BusinessTwin.Server.Modules.Purchasing;
using BusinessTwin.Server.Modules.Inventory;

namespace BusinessTwin.Server.Tests;

public sealed class PurchasingServiceTests
{
    [Fact]
    public void ApprovedRequest_CanCreatePurchaseOrder()
    {
        var service = new PurchasingService();
        var request = service.CreateRequest(
            new CreatePurchaseRequest(Guid.NewGuid(), [new PurchaseLineRequest(Guid.NewGuid(), 10, 25m)]),
            DateTimeOffset.UtcNow,
            "tester");

        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.PendingApproval);
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.Approved);
        var order = service.CreateOrderFromApprovedRequest(request.Id, DateTimeOffset.UtcNow, "tester");

        Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        Assert.Equal(250m, order.Total);
    }

    [Fact]
    public void DraftRequest_CannotCreatePurchaseOrder()
    {
        var service = new PurchasingService();
        var request = service.CreateRequest(
            new CreatePurchaseRequest(Guid.NewGuid(), [new PurchaseLineRequest(Guid.NewGuid(), 10, 25m)]),
            DateTimeOffset.UtcNow,
            "tester");

        var action = () => service.CreateOrderFromApprovedRequest(request.Id, DateTimeOffset.UtcNow, "tester");

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void PurchaseOrder_TracksPartialPayment()
    {
        var service = new PurchasingService();
        var request = service.CreateRequest(
            new CreatePurchaseRequest(Guid.NewGuid(), [new PurchaseLineRequest(Guid.NewGuid(), 10, 25m)]),
            DateTimeOffset.UtcNow,
            "tester");
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.PendingApproval);
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.Approved);
        var order = service.CreateOrderFromApprovedRequest(request.Id, DateTimeOffset.UtcNow, "tester");

        order.RecordPayment(100m);

        Assert.Equal(PaymentStatus.PartiallyPaid, order.PaymentStatus);
    }

    [Fact]
    public void Receive_UpdatesInventoryAndCompletesOrderWhenFullyReceived()
    {
        var service = new PurchasingService();
        var inventory = new InventoryService();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        inventory.CreateBalance(productId, warehouseId, 0, DateTimeOffset.UtcNow, "seed");
        var request = service.CreateRequest(
            new CreatePurchaseRequest(Guid.NewGuid(), [new PurchaseLineRequest(productId, 10, 25m)]),
            DateTimeOffset.UtcNow,
            "tester");
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.PendingApproval);
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.Approved);
        var order = service.CreateOrderFromApprovedRequest(request.Id, DateTimeOffset.UtcNow, "tester");
        order.ChangeStatus(PurchaseOrderStatus.Ordered);

        service.Receive(order.Id, new ReceivePurchaseRequest(productId, warehouseId, 10), inventory, DateTimeOffset.UtcNow, "tester");

        Assert.Equal(PurchaseOrderStatus.Received, order.Status);
        Assert.Equal(10, inventory.GetBalance(productId, warehouseId).Quantity);
    }

    [Fact]
    public void Receive_CannotExceedOrderedQuantity()
    {
        var service = new PurchasingService();
        var inventory = new InventoryService();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        inventory.CreateBalance(productId, warehouseId, 0, DateTimeOffset.UtcNow, "seed");
        var request = service.CreateRequest(
            new CreatePurchaseRequest(Guid.NewGuid(), [new PurchaseLineRequest(productId, 10, 25m)]),
            DateTimeOffset.UtcNow,
            "tester");
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.PendingApproval);
        service.ChangeRequestStatus(request.Id, PurchaseRequestStatus.Approved);
        var order = service.CreateOrderFromApprovedRequest(request.Id, DateTimeOffset.UtcNow, "tester");
        order.ChangeStatus(PurchaseOrderStatus.Ordered);

        var action = () => service.Receive(order.Id, new ReceivePurchaseRequest(productId, warehouseId, 11), inventory, DateTimeOffset.UtcNow, "tester");

        Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(0, inventory.GetBalance(productId, warehouseId).Quantity);
    }
}