using BusinessTwin.Server.Modules.Purchasing;

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
}