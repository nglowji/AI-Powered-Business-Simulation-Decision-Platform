namespace BusinessTwin.Server.Modules.Ai;

public sealed class AiAssistantService
{
    public AiAssistantResponse Answer(AiAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return new AiAssistantResponse(
                "unknown",
                "Question is required.",
                [],
                false);
        }

        var question = request.Question.Trim().ToLowerInvariant();
        var facts = request.Facts;

        if (ContainsAny(question, "doanh thu", "revenue"))
        {
            return CreateMetricResponse(
                "revenue_analysis",
                facts?.Revenue,
                "Revenue",
                facts?.AsOfDate,
                "No realtime revenue data was provided. The assistant will not guess.");
        }

        if (ContainsAny(question, "lợi nhuận", "loi nhuan", "profit"))
        {
            return CreateMetricResponse(
                "profit_analysis",
                facts?.Profit,
                "Profit",
                facts?.AsOfDate,
                "No realtime profit data was provided. The assistant will not guess.");
        }

        if (ContainsAny(question, "tồn kho", "ton kho", "inventory", "kho"))
        {
            return CreateMetricResponse(
                "inventory_analysis",
                facts?.InventoryValue,
                "Inventory value",
                facts?.AsOfDate,
                "No realtime inventory data was provided. The assistant will not guess.");
        }

        return new AiAssistantResponse(
            "unknown",
            "The question is not mapped to a business tool yet.",
            [],
            true);
    }

    private static AiAssistantResponse CreateMetricResponse(
        string intent,
        decimal? value,
        string metricName,
        DateOnly? asOfDate,
        string missingDataMessage)
    {
        if (value is null)
        {
            return new AiAssistantResponse(intent, missingDataMessage, [], true);
        }

        var dateText = asOfDate?.ToString("yyyy-MM-dd") ?? "the supplied date";
        return new AiAssistantResponse(
            intent,
            $"{metricName} is {value.Value:N2} as of {dateText}.",
            ["business-facts-tool"],
            false);
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(value.Contains);
    }
}