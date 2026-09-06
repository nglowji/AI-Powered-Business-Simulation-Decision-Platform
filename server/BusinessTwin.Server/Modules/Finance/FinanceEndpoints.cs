namespace BusinessTwin.Server.Modules.Finance;

public static class FinanceEndpoints
{
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/finance").WithTags("Finance");

        group.MapPost("/entries", (FinancialEntryRequest request, FinanceService service) =>
        {
            try
            {
                var entry = service.AddEntry(request, DateTimeOffset.UtcNow, "system");
                return Results.Created($"/api/v1/finance/entries/{entry.Id}", entry);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/summary", (FinanceService service) => Results.Ok(service.GetSummary()));

        return endpoints;
    }
}