namespace BusinessTwin.Server.Modules.Ai;

public sealed class AiAssistantService
{
    public AiAssistantResponse Answer(AiAssistantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return new AiAssistantResponse(
                "unknown",
                "Vui lòng nhập câu hỏi.",
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
                "Doanh thu",
                facts?.AsOfDate,
                "Chưa có dữ liệu doanh thu realtime. Trợ lý sẽ không tự suy đoán.");
        }

        if (ContainsAny(question, "lợi nhuận", "loi nhuan", "profit"))
        {
            return CreateMetricResponse(
                "profit_analysis",
                facts?.Profit,
                "Lợi nhuận",
                facts?.AsOfDate,
                "Chưa có dữ liệu lợi nhuận realtime. Trợ lý sẽ không tự suy đoán.");
        }

        if (ContainsAny(question, "tồn kho", "ton kho", "inventory", "kho"))
        {
            return CreateMetricResponse(
                "inventory_analysis",
                facts?.InventoryValue,
                "Giá trị tồn kho",
                facts?.AsOfDate,
                "Chưa có dữ liệu tồn kho realtime. Trợ lý sẽ không tự suy đoán.");
        }

        return new AiAssistantResponse(
            "unknown",
            "Câu hỏi này chưa được kết nối với business tool phù hợp.",
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

        var dateText = asOfDate?.ToString("dd/MM/yyyy") ?? "ngày được cung cấp";
        return new AiAssistantResponse(
            intent,
            $"{metricName} hiện là {value.Value:N2} tại ngày {dateText}.",
            ["business-facts-tool"],
            false);
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(value.Contains);
    }
}