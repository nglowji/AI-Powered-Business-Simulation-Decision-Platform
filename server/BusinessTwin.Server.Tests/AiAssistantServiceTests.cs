using BusinessTwin.Server.Modules.Ai;

namespace BusinessTwin.Server.Tests;

public sealed class AiAssistantServiceTests
{
    [Fact]
    public void Answer_RevenueQuestionWithFacts_ReturnsProvidedValue()
    {
        var service = new AiAssistantService();
        var request = new AiAssistantRequest(
            "Doanh thu tháng này thế nào?",
            new BusinessFacts(2_420_000m, null, 3421, null, new DateOnly(2026, 9, 6)));

        var response = service.Answer(request);

        Assert.Equal("revenue_analysis", response.Intent);
        Assert.Contains("2,420,000.00", response.Answer);
        Assert.False(response.RequiresRealtimeData);
    }

    [Fact]
    public void Answer_RevenueQuestionWithoutFacts_DoesNotGuess()
    {
        var service = new AiAssistantService();

        var response = service.Answer(new AiAssistantRequest("Revenue this month?", null));

        Assert.True(response.RequiresRealtimeData);
        Assert.Contains("will not guess", response.Answer);
        Assert.Empty(response.Sources);
    }
}