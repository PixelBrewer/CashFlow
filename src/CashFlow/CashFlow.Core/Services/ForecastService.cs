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
        ValidateInputs(
            scheduledTransactions,
            recurringTransactions,
            from,
            through
        );

        var scheduledTransactionsWithinRange =
            GetScheduledTransactionsWithinRange(
                scheduledTransactions,
                from,
                through
            );

        var generatedRecurringTransactions =
            GenerateRecurringTransactions(
                recurringTransactions,
                from,
                through
            );

        var allTransactions = scheduledTransactionsWithinRange
            .Concat(generatedRecurringTransactions)
            .ToList();

        return projectionService.GenerateProjection(
            openingBalance,
            allTransactions
        );
    }

    private static void ValidateInputs(
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
            throw new ArgumentException(
                "The start date cannot be after the end date."
            );
        }
    }

    private static IReadOnlyList<ScheduledTransaction>
        GetScheduledTransactionsWithinRange(
            IEnumerable<ScheduledTransaction> transactions,
            DateOnly from,
            DateOnly through
        )
    {
        return transactions
            .Where(transaction =>
                transaction.Date >= from
                && transaction.Date <= through
            )
            .ToList();
    }

    private IReadOnlyList<ScheduledTransaction>
        GenerateRecurringTransactions(
            IEnumerable<RecurringTransaction> recurringTransactions,
            DateOnly from,
            DateOnly through
        )
    {
        return recurringTransactions
            .SelectMany(transaction =>
                recurringTransactionService.Generate(
                    transaction,
                    from,
                    through
                )
            )
            .ToList();
    }
}
