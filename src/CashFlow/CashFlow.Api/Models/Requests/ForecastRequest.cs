namespace CashFlow.Api.Models.Requests;

using CashFlow.Core.Models;

public class ForecastRequest
{
    public decimal OpeningBalance { get; set; }
    public DateOnly From { get; set; }
    public DateOnly Through { get; set; }

    public IReadOnlyList<ScheduledTransaction> ScheduledTransactions { get; set; } = [];
    public IReadOnlyList<RecurringTransaction> RecurringTransactions { get; set; } = [];
}
