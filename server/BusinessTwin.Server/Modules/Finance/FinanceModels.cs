using BusinessTwin.Server.Shared;

namespace BusinessTwin.Server.Modules.Finance;

public enum FinancialEntryType
{
    Revenue,
    Expense,
    CustomerPayment,
    SupplierPayment
}

public sealed record FinancialEntryRequest(
    FinancialEntryType Type,
    decimal Amount,
    string Reference,
    string Description);

public sealed class FinancialEntry : EntityAudit
{
    public FinancialEntry(
        Guid id,
        FinancialEntryType type,
        decimal amount,
        string reference,
        string description,
        DateTimeOffset createdAt,
        string createdBy)
        : base(id, createdAt, createdBy)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Financial entry amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Financial entry reference is required.", nameof(reference));
        }

        Type = type;
        Amount = amount;
        Reference = reference;
        Description = description;
    }

    public FinancialEntryType Type { get; }

    public decimal Amount { get; }

    public string Reference { get; }

    public string Description { get; }
}

public sealed record FinanceSummaryResponse(
    decimal Revenue,
    decimal Expenses,
    decimal Profit,
    decimal CustomerPayments,
    decimal SupplierPayments,
    decimal CashFlow,
    decimal Receivables,
    decimal Payables);