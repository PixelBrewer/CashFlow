namespace CashFlow.Core.Models;

using CashFlow.Core.Enums;

public sealed class RecurringTransaction
{
    public Guid Id { get; set; }
    public required string Description { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public RecurrenceFrequency Frequency { get; set; }

    public DateOnly StartDate { get; set; }
}
