namespace BusinessTwin.Server.Modules.Sales;

public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/sales-orders").WithTags("Sales");

        group.MapPost("", (CreateSalesOrderRequest request, SalesService service) =>
        {
            try
            {
                var order = service.CreateOrder(request, DateTimeOffset.UtcNow, "system");
                return Results.Created($"/api/v1/sales-orders/{order.Id}", order);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/{orderId:guid}", (Guid orderId, SalesService service) =>
        {
            try
            {
                return Results.Ok(service.GetOrder(orderId));
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
        });

        group.MapPost("/{orderId:guid}/status", (Guid orderId, SalesOrderStatus status, SalesService service) =>
        {
            try
            {
                return Results.Ok(service.ChangeStatus(orderId, status));
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