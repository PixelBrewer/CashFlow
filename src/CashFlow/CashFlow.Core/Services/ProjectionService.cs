namespace CashFlow.Core.Services;

using CashFlow.Core.Enums;
using CashFlow.Core.Models;

public interface IProjectionService
{
    Projection GenerateProjection(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> transactions
    );
}

public class ProjectionService : IProjectionService
{
    public Projection GenerateProjection(
        decimal openingBalance,
        IEnumerable<ScheduledTransaction> transactions
    )
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var orderedTransactions = OrderTransactions(transactions);

        var runningBalance = openingBalance;
        var lowestBalance = openingBalance;
        var entries = new List<Entry>();

        foreach (var transaction in orderedTransactions)
        {
            ValidateTransaction(transaction);

            runningBalance = ApplyTransaction(
                runningBalance,
                transaction
            );

            lowestBalance = Math.Min(
                lowestBalance,
                runningBalance
            );

            entries.Add(
                CreateEntry(
                    transaction,
                    runningBalance
                )
            );
        }

        return new Projection
        {
            OpeningBalance = openingBalance,
            EndingBalance = runningBalance,
            LowestBalance = lowestBalance,
            Entries = entries,
        };
    }

    private static IReadOnlyList<ScheduledTransaction> OrderTransactions(
        IEnumerable<ScheduledTransaction> transactions
    )
    {
        return transactions
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Description)
            .ToList();
    }

    private static decimal ApplyTransaction(
        decimal balance,
        ScheduledTransaction transaction
    )
    {
        return transaction.Type switch
        {
            TransactionType.Income =>
                balance + transaction.Amount,

            TransactionType.Expense =>
                balance - transaction.Amount,

            _ => throw new ArgumentOutOfRangeException(
                nameof(transaction),
                transaction.Type,
                "Transaction contains an invalid transaction type."
            ),
        };
    }

    private static Entry CreateEntry(
        ScheduledTransaction transaction,
        decimal balance
    )
    {
        return new Entry
        {
            Transaction = transaction,
            BalanceAfterTransaction = balance,
        };
    }

    private static void ValidateTransaction(
        ScheduledTransaction transaction
    )
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (string.IsNullOrWhiteSpace(transaction.Description))
        {
            throw new ArgumentException(
                "Transaction description is required",
                nameof(transaction)
            );
        }

        if (transaction.Amount < 0)
        {
            throw new ArgumentException(
                "Transaction amount cannot be negative",
                nameof(transaction)
            );
        }
    }
}
