using BusinessTwin.Server.Modules.Finance;

namespace BusinessTwin.Server.Tests;

public sealed class FinanceServiceTests
{
    [Fact]
    public void GetSummary_CalculatesProfitCashFlowAndOutstandingBalances()
    {
        var service = new FinanceService();
        service.AddEntry(new FinancialEntryRequest(FinancialEntryType.Revenue, 1000m, "SO-1", "Sale"), DateTimeOffset.UtcNow, "tester");
        service.AddEntry(new FinancialEntryRequest(FinancialEntryType.Expense, 400m, "PO-1", "Purchase"), DateTimeOffset.UtcNow, "tester");
        service.AddEntry(new FinancialEntryRequest(FinancialEntryType.CustomerPayment, 800m, "PAY-1", "Customer payment"), DateTimeOffset.UtcNow, "tester");
        service.AddEntry(new FinancialEntryRequest(FinancialEntryType.SupplierPayment, 250m, "PAY-2", "Supplier payment"), DateTimeOffset.UtcNow, "tester");

        var summary = service.GetSummary();

        Assert.Equal(1000m, summary.Revenue);
        Assert.Equal(400m, summary.Expenses);
        Assert.Equal(600m, summary.Profit);
        Assert.Equal(550m, summary.CashFlow);
        Assert.Equal(200m, summary.Receivables);
        Assert.Equal(150m, summary.Payables);
    }

    [Fact]
    public void AddEntry_RejectsNonPositiveAmount()
    {
        var service = new FinanceService();

        var action = () => service.AddEntry(
            new FinancialEntryRequest(FinancialEntryType.Expense, 0m, "EXP-1", "Invalid"),
            DateTimeOffset.UtcNow,
            "tester");

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
}