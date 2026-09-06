namespace BusinessTwin.Server.Modules.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/inventory").WithTags("Inventory");

        group.MapGet("/{productId:guid}/{warehouseId:guid}", (
            Guid productId,
            Guid warehouseId,
            InventoryService service) =>
        {
            try
            {
                return Results.Ok(service.GetBalance(productId, warehouseId));
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
        });

        group.MapPost("/transactions", (
            InventoryTransactionRequest request,
            InventoryService service) =>
        {
            try
            {
                var transaction = service.ApplyTransaction(request, DateTimeOffset.UtcNow, "system");
                return Results.Created($"/api/v1/inventory/transactions/{transaction.Id}", transaction);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        return endpoints;
    }
}