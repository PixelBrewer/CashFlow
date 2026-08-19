namespace CashFlow.Core.Models;

public sealed class Projection
{
    public decimal OpeningBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public IReadOnlyList<Entry> Entries { get; set; } = [];
}
