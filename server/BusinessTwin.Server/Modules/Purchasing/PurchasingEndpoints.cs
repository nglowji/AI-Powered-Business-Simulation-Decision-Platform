namespace BusinessTwin.Server.Modules.Purchasing;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/purchasing").WithTags("Purchasing");

        group.MapPost("/requests", (CreatePurchaseRequest request, PurchasingService service) =>
        {
            try
            {
                var purchaseRequest = service.CreateRequest(request, DateTimeOffset.UtcNow, "system");
                return Results.Created($"/api/v1/purchasing/requests/{purchaseRequest.Id}", purchaseRequest);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapPost("/requests/{requestId:guid}/status", (Guid requestId, PurchaseRequestStatus status, PurchasingService service) =>
        {
            try
            {
                return Results.Ok(service.ChangeRequestStatus(requestId, status));
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        group.MapPost("/requests/{requestId:guid}/purchase-order", (Guid requestId, PurchasingService service) =>
        {
            try
            {
                var order = service.CreateOrderFromApprovedRequest(requestId, DateTimeOffset.UtcNow, "system");
                return Results.Created($"/api/v1/purchasing/orders/{order.Id}", order);
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        });

        return endpoints;
    }
}