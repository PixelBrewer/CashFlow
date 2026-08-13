namespace CashFlow.Infrastructure.Excel;

using CashFlow.Core.Interfaces;
using CashFlow.Core.Models;

public class ExcelCashFlowBudgetProvider(string filePath) : ICashFlowBudgetProvider
{
    public CashFlowBudgetDefinition GetBudget()
    {
        throw new NotImplementedException();
    }
}
