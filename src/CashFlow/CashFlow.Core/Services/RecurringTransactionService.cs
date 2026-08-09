namespace CashFlow.Core.Services;

using CashFlow.Core.Enums;
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
    public IReadOnlyList<ScheduledTransaction> Generate(
        RecurringTransaction transaction,
        DateOnly from,
        DateOnly through
    )
    {
        var results = new List<ScheduledTransaction>();

        var recurrenceDay = transaction.StartDate.Day;

        var currentMonth = new DateOnly(transaction.StartDate.Year, transaction.StartDate.Month, 1);

        if (from > through)
        {
            throw new ArgumentException("The start date cannot be after the current date.");
        }

        while (currentMonth <= through)
        {
            var daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
            var day = Math.Min(recurrenceDay, daysInMonth);
            var occurrenceDate = new DateOnly(currentMonth.Year, currentMonth.Month, day);
            if (
                occurrenceDate >= transaction.StartDate
                && occurrenceDate >= from
                && occurrenceDate <= through
            )
            {
                results.Add(
                    new ScheduledTransaction
                    {
                        Id = Guid.NewGuid(),
                        Description = transaction.Description,
                        Date = occurrenceDate,
                        Amount = transaction.Amount,
                        Type = transaction.Type,
                    }
                );
            }
            currentMonth = currentMonth.AddMonths(1);
        }
        return results;
    }

    private static DateOnly GetNextOccurrence(
        DateOnly current,
        RecurrenceFrequency frequency,
        int recurrenceDay
    )
    {
        return frequency switch
        {
            RecurrenceFrequency.Weekly => current.AddDays(7),

            RecurrenceFrequency.Biweekly => current.AddDays(14),

            RecurrenceFrequency.Monthly => current.AddMonths(1),

            _ => throw new ArgumentOutOfRangeException(nameof(frequency)),
        };
    }
}
