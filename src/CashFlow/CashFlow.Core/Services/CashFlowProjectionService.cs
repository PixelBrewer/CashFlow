namespace CashFlow.Core.Services;

using Models;


public interface ICashFlowProjectionService
{
    CashFlowProjection GenerateProjection(decimal openingBalance, IEnumerable<ScheduledTransaction> transactions);
}

public class CashFlowProjectionService : ICashFlowProjectionService
{
    public CashFlowProjection GenerateProjection(decimal openingBalance, IEnumerable<ScheduledTransaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        
        var orderedTransactions = transactions.OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Description)
            .ToList();
        
        var runningBalance = openingBalance;
        var lowestBalance = openingBalance;
        var entries = new List<CashFlowEntry>();

        foreach (var transaction in orderedTransactions)
        {
            Val
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        return new CashFlowProjection
        {
            OpeningBalance = openingBalance,
            EndingBalance = runningBalance,
            LowestBalance = lowestBalance,
            Entries = entries
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
            throw new ArgumentException("Transaction amount cannot be negative", nameof(transaction));
        }
    }
}