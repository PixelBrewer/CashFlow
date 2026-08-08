namespace CashFlow.Core.Services;

using CashFlow.Core.Models;

public interface IRecurringTransactionService
{
    public IReadOnlyList<ScheduledTransaction> Generate(
        RecurringTransaction transaction,
        DateOnly from,
        DateOnly through
    );
}

public class RecurringTransactionService : IRecurringTransactionService
{
    public IReadonlyList<ScheduledTransaction> Generate(
        RecurringTransaction transaction,
        DateOnly from,
        DateOnly through
    )
    {
        throw new NotImplementedException();
    }
}
