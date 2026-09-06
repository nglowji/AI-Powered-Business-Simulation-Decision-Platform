namespace BusinessTwin.Server.Modules.Simulation;

public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapSimulationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/simulation").WithTags("Simulation");

        group.MapPost("/run", (RunSimulationRequest request, SimulationService service) =>
        {
            try
            {
                return Results.Created("/api/v1/simulation/scenarios", service.Run(request, DateTimeOffset.UtcNow));
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/scenarios", (SimulationService service) => Results.Ok(service.GetScenarios()));

        return endpoints;
    }
}