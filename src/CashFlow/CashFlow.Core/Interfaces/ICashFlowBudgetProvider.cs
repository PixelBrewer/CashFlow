using CashFlow.Core.Models;

namespace CashFlow.Core.Interfaces;

public interface ICashFlowBudgetProvider
{
    CashFlowBudgetDefinition GetBudget(DateOnly effectiveDate);
}
