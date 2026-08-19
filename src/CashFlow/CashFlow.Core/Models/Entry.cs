namespace CashFlow.Core.Models;

public sealed class Entry
{
    public required ScheduledTransaction Transaction { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
}
