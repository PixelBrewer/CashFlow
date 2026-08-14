namespace CashFlow.Infrastructure.Excel;

using CashFlow.Core.Enums;
using CashFlow.Core.Interfaces;
using CashFlow.Core.Models;
using ClosedXML.Excel;

public class ExcelCashFlowBudgetProvider(string filePath) : ICashFlowBudgetProvider
{
    public CashFlowBudgetDefinition GetBudget(DateOnly effectiveDate)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet("Budget");

        var recurringTransactions = new List<RecurringTransaction>();
        var lastRowUsed = worksheet.LastRowUsed();

        if (lastRowUsed is null)
        {
            return new CashFlowBudgetDefinition();
        }

        var lastRow = lastRowUsed.RowNumber();

        for (var row = 2; row <= lastRow; row++)
        {
            var dueDateCell = worksheet.Cell(row, 1);
            var nameCell = worksheet.Cell(row, 2);
            var amountCell = worksheet.Cell(row, 3);

            if (!IsValidBillRow(dueDateCell, nameCell, amountCell))
            {
                continue;
            }

            var dueDate = dueDateCell.GetValue<int>();
            var name = nameCell.GetString();
            var amount = amountCell.GetValue<decimal>();

            recurringTransactions.Add(
                new RecurringTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = name,
                    Amount = amount,
                    Type = TransactionType.Expense,
                    Frequency = RecurrenceFrequency.Monthly,
                    StartDate = GetNextMonthlyOccurrence(effectiveDate, dueDate),
                }
            );
        }
        return new CashFlowBudgetDefinition { RecurringTransactions = recurringTransactions };
    }

    private static DateOnly GetNextMonthlyOccurrence(DateOnly effectiveDate, int dayOfMonth)
    {
        var daysInCurrentMonth = DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month);
        var currentMonthDay = Math.Min(dayOfMonth, daysInCurrentMonth);
        var candidate = new DateOnly(effectiveDate.Year, effectiveDate.Month, currentMonthDay);

        if (candidate >= effectiveDate)
        {
            return candidate;
        }

        var nextMonth = effectiveDate.AddMonths(1);
        var daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);

        var nextMonthDay = Math.Min(dayOfMonth, daysInNextMonth);
        return new DateOnly(nextMonth.Year, nextMonth.Month, nextMonthDay);
    }

    private static bool IsValidBillRow(IXLCell dueDateCell, IXLCell nameCell, IXLCell amountCell)
    {
        return !dueDateCell.IsEmpty() && !nameCell.IsEmpty() && !amountCell.IsEmpty();
    }
}
