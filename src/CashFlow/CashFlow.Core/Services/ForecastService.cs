namespace CashFlow.Core.Services;

using CashFlow.Core.Models;

public interface IForecastService
{
    Projection GenerateForecast(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> scheduledTransactions,
        IEnumerable<RecurringTransaction> recurringTransactions,
        DateOnly from,
        DateOnly through
    );
}

public class ForecastService(
    IRecurringTransactionService recurringTransactionService,
    IProjectionService projectionService
) : IForecastService
{
    public Projection GenerateForecast(
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
            var generatedTransactions = recurringTransactionService.Generate(
                recurringTransaction,
                from,
                through
            );
            allTransactions.AddRange(generatedTransactions);
        }
        return projectionService.GenerateProjection(openingBalance, allTransactions);
    }
}
