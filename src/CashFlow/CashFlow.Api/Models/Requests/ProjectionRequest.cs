namespace CashFlow.Api.Models.Requests;

using CashFlow.Core.Models;

public class ProjectionRequest
{
    public decimal OpeningBalance { get; set; }
    public IReadOnlyList<ScheduledTransaction> Transactions { get; set; } = [];
}
