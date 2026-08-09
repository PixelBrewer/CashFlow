namespace CashFlow.Core.Services;

using CashFlow.Core.Models;

public interface ICashFlowForecastService
{
    CashFlowProjection GenerateForecast(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> scheduledTransactions,
        IEnumerable<RecurringTransaction> recurringTrasnactions,
        DateOnly from,
        DateOnly through
    );
}

public class CashFlowForecastService(
    IRecurringTransactionService recurringTransactionService,
    ICashFlowProjectionService cashFlowProjectionService
) : ICashFlowForecastService
{
    public CashFlowProjection GenerateForecast(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> scheduledTransactions,
        IEnumerable<RecurringTransaction> recurringTransactions,
        DateOnly from,
        DateOnly through
    )
    {
        throw new NotImplementedException();
    }
}
