using CashFlow.Core.Models;

namespace CashFlow.Core.Interfaces;

public interface IBudgetProvider
{
    BudgetDefinition GetBudget(DateOnly effectiveDate);
}
