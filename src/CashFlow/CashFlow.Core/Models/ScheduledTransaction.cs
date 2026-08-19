namespace CashFlow.Core.Models;

using Enums;

public sealed class ScheduledTransaction
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
}