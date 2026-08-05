namespace CashFlow.Core.Models;

public sealed class CashFlowEntry
{
    public required ScheduledTransaction Transaction { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
}