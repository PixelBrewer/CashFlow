using CashFlow.Core.Enums;

namespace CashFlow.Core.Services;

using Models;

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
        var orderedTransactions = transactions
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Description)
            .ToList();
        var runningBalance = openingBalance;
        var lowestBalance = openingBalance;
        var entries = new List<Entry>();

        foreach (var transaction in orderedTransactions)
        {
            ValidateTransaction(transaction);
            runningBalance = transaction.Type switch
            {
                TransactionType.Income => runningBalance + transaction.Amount,

                TransactionType.Expense => runningBalance - transaction.Amount,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(transactions),
                    transaction.Type,
                    "Transaction contains an invalid transaction type."
                ),
            };
            lowestBalance = Math.Min(lowestBalance, runningBalance);
            entries.Add(
                new Entry { Transaction = transaction, BalanceAfterTransaction = runningBalance }
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

    private static void ValidateTransaction(ScheduledTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (string.IsNullOrWhiteSpace(transaction.Description))
        {
            throw new ArgumentException("Transaction description is required", nameof(transaction));
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
