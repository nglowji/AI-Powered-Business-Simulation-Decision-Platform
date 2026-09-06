namespace BusinessTwin.Server.Modules.Ai;

public static class AiAnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAiAnalyticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/ai").WithTags("AI");

        group.MapPost("/forecast", (AiAnalyticsRequest request, AiAnalyticsService service) =>
            Results.Ok(service.Forecast(request)));

        group.MapPost("/recommendations", (AiAnalyticsRequest request, AiAnalyticsService service) =>
            Results.Ok(new
            {
                recommendations = service.Recommend(request),
                source = "business-facts-tool"
            }));

        return endpoints;
    }
}