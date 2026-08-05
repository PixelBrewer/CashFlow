namespace CashFlow.Core.Models;

public sealed class CashFlowProjection
{
    public decimal OpeningBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public IReadOnlyList<CashFlowEntry> Entries { get; set; } = Array.Empty<CashFlowEntry>();
}