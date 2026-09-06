namespace BusinessTwin.Server.Modules.DigitalTwin;

public static class DigitalTwinEndpoints
{
    public static IEndpointRouteBuilder MapDigitalTwinEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/digital-twin").WithTags("Digital Twin");

        group.MapPost("/snapshots", (CreateSnapshotRequest request, DigitalTwinService service) =>
        {
            try
            {
                var snapshot = service.CreateSnapshot(request, DateTimeOffset.UtcNow);
                return Results.Created($"/api/v1/digital-twin/snapshots/{snapshot.Version}", snapshot);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        });

        group.MapGet("/current", (DigitalTwinService service) =>
        {
            try
            {
                return Results.Ok(service.GetCurrent());
            }
            catch (InvalidOperationException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
        });

        group.MapGet("/timeline", (DigitalTwinService service) => Results.Ok(service.GetTimeline()));

        group.MapGet("/compare/{fromVersion:int}/{toVersion:int}", (
            int fromVersion,
            int toVersion,
            DigitalTwinService service) =>
        {
            try
            {
                return Results.Ok(service.Compare(fromVersion, toVersion));
            }
            catch (KeyNotFoundException exception)
            {
                return Results.NotFound(new { error = exception.Message });
            }
        });

        return endpoints;
    }
}