using BusinessTwin.Server.Modules.Inventory;

namespace BusinessTwin.Server.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public void ApplyTransaction_OutBeyondAvailableQuantity_ThrowsConflict()
    {
        var service = CreateService(out var productId, out var warehouseId);

        var action = () => service.ApplyTransaction(
            new InventoryTransactionRequest(
                productId,
                warehouseId,
                InventoryTransactionType.Out,
                101,
                "SO-001"),
            DateTimeOffset.UtcNow,
            "tester");

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("exceeds available", exception.Message);
    }

    [Fact]
    public void ReserveBeyondAvailableQuantity_ThrowsConflict()
    {
        var service = CreateService(out var productId, out var warehouseId);

        var action = () => service.Reserve(productId, warehouseId, 101);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("not available", exception.Message);
    }

    [Fact]
    public void ApplyTransaction_IncreasesBalanceAndCreatesAuditRecord()
    {
        var service = CreateService(out var productId, out var warehouseId);

        var transaction = service.ApplyTransaction(
            new InventoryTransactionRequest(
                productId,
                warehouseId,
                InventoryTransactionType.In,
                25,
                "PO-001"),
            DateTimeOffset.UtcNow,
            "tester");

        var balance = service.GetBalance(productId, warehouseId);

        Assert.Equal(InventoryTransactionType.In, transaction.Type);
        Assert.Equal("tester", transaction.Actor);
        Assert.Equal(125, balance.Quantity);
        Assert.Single(service.GetTransactions());
    }

    private static InventoryService CreateService(out Guid productId, out Guid warehouseId)
    {
        productId = Guid.NewGuid();
        warehouseId = Guid.NewGuid();
        var service = new InventoryService();
        service.CreateBalance(productId, warehouseId, 100, DateTimeOffset.UtcNow, "seed");
        return service;
    }
}