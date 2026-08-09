namespace CashFlow.Core.Services;

using CashFlow.Core.Enums;
using CashFlow.Core.Models;

public interface IRecurringTransactionService
{
    IReadOnlyList<ScheduledTransaction> Generate(
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
        if (from > through)
        {
            throw new ArgumentException("The start date cannot be after the end date.");
        }

        var results = new List<ScheduledTransaction>();

        var recurrenceDay = transaction.StartDate.Day;
        var occurrenceDate = transaction.StartDate;

        while (occurrenceDate <= through)
        {
            if (occurrenceDate >= from)
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

            occurrenceDate = GetNextOccurrence(
                occurrenceDate,
                transaction.Frequency,
                recurrenceDay
            );
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

            RecurrenceFrequency.Monthly => GetNextMonthlyOccurrence(current, recurrenceDay),

            _ => throw new ArgumentOutOfRangeException(
                nameof(frequency),
                frequency,
                "Unsupported recurrence frequency."
            ),
        };
    }

    private static DateOnly GetNextMonthlyOccurrence(DateOnly current, int recurrenceDay)
    {
        var nextMonth = current.AddMonths(1);

        var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);

        var day = Math.Min(recurrenceDay, daysInMonth);

        return new DateOnly(nextMonth.Year, nextMonth.Month, day);
    }
}
