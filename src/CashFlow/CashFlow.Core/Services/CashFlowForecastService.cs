namespace CashFlow.Core.Services;

using CashFlow.Core.Models;

public interface ICashFlowForecastService
{
    CashFlowProjection GenerateForecast(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> scheduledTransactions,
        IEnumerable<RecurringTransaction> recurringTransactions,
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
        ArgumentNullException.ThrowIfNull(scheduledTransactions);
        ArgumentNullException.ThrowIfNull(recurringTransactions);

        if (from > through)
        {
            throw new ArgumentException("The start date cannot be after the end date.");
        }
        var allTransactions = scheduledTransactions
            .Where(transaction => transaction.Date >= from && transaction.Date <= through)
            .ToList();

        foreach (var recurringTransaction in recurringTransactions)
        {
            var generateTransactions = recurringTransactionService.Generate(
                recurringTransaction,
                from,
                through
            );
            allTransactions.AddRange(generateTransactions);
        }
        return cashFlowProjectionService.GenerateProjection(openingBalance, allTransactions);
    }
}
