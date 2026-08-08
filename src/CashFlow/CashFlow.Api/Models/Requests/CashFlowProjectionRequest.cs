namespace CashFlow.Api.Models.Requests;

using CashFlow.Core.Models;

public class CashFlowProjectionRequest
{
    public decimal OpeningBalance { get; set; }
    public IReadOnlyList<ScheduledTransaction> Transactions { get; set; } = [];
}
