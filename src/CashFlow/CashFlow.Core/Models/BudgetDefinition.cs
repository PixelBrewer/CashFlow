namespace CashFlow.Core.Models;

public class BudgetDefinition
{
    public IReadOnlyList<ScheduledTransaction> ScheduledTransactions { get; set; } = [];
    public IReadOnlyList<RecurringTransaction> RecurringTransactions { get; set; } = [];
}
