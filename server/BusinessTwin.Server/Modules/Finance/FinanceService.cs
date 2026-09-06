namespace BusinessTwin.Server.Modules.Finance;

public sealed class FinanceService
{
    private readonly List<FinancialEntry> entries = [];

    public FinancialEntry AddEntry(
        FinancialEntryRequest request,
        DateTimeOffset createdAt,
        string createdBy)
    {
        var entry = new FinancialEntry(
            Guid.NewGuid(),
            request.Type,
            request.Amount,
            request.Reference,
            request.Description,
            createdAt,
            createdBy);
        entries.Add(entry);
        return entry;
    }

    public FinanceSummaryResponse GetSummary()
    {
        var revenue = Sum(FinancialEntryType.Revenue);
        var expenses = Sum(FinancialEntryType.Expense);
        var customerPayments = Sum(FinancialEntryType.CustomerPayment);
        var supplierPayments = Sum(FinancialEntryType.SupplierPayment);

        return new FinanceSummaryResponse(
            revenue,
            expenses,
            revenue - expenses,
            customerPayments,
            supplierPayments,
            customerPayments - supplierPayments,
            revenue - customerPayments,
            expenses - supplierPayments);
    }

    private decimal Sum(FinancialEntryType type)
    {
        return entries
            .Where(entry => entry.Type == type)
            .Sum(entry => entry.Amount);
    }
}