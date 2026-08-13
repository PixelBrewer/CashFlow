namespace CashFlow.Infrastructure.Excel;

using CashFlow.Core.Enums;
using CashFlow.Core.Interfaces;
using CashFlow.Core.Models;
using ClosedXML.Excel;

public class ExcelCashFlowBudgetProvider(string filePath) : ICashFlowBudgetProvider
{
    public CashFlowBudgetDefinition GetBudget()
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet("Budget");

        var recurringTransactions = new List<RecurringTransaction>();

        var name = worksheet.Cell("B2").GetString();
        var amount = worksheet.Cell("C2").GetValue<decimal>();

        recurringTransactions.Add(
            new RecurringTransaction
            {
                Id = Guid.NewGuid(),
                Description = name,
                Amount = amount,
                Type = TransactionType.Expense,
                Frequency = RecurrenceFrequency.Monthly,
            }
        );

        return new CashFlowBudgetDefinition { RecurringTransactions = recurringTransactions };
    }
}
