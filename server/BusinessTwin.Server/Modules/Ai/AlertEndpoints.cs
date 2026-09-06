namespace BusinessTwin.Server.Modules.Ai;

public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapAlertEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/ai").WithTags("AI Alerts");

        group.MapPost("/alerts/evaluate", (AlertFacts facts, AlertService service) =>
            Results.Ok(new
            {
                alerts = service.Evaluate(facts),
                source = "business-facts-tool"
            }));

        return endpoints;
    }
}