namespace CashFlow.Core.Models;

public class CashFlowBudgetDefinition
{
    public IReadOnlyList<ScheduledTransaction> ScheduledTransactions { get; set; } = [];
    public IReadOnlyList<RecurringTransaction> RecurringTransactions { get; set; } = [];
}
