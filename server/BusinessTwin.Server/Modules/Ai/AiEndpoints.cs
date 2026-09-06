namespace BusinessTwin.Server.Modules.Ai;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/ai")
            .WithTags("AI");

        group.MapPost("/assistant", (AiAssistantRequest request, AiAssistantService service) =>
        {
            var response = service.Answer(request);
            return Results.Ok(response);
        });

        return endpoints;
    }
}