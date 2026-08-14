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

        var worksheet = workbook.Worksheet("Sheet1");

        var recurringTransactions = new List<RecurringTransaction>();
        var lastRowUsed = worksheet.LastRowUsed();

        if (lastRowUsed is null)
        {
            return new CashFlowBudgetDefinition();
        }

        var lastRow = lastRowUsed.RowNumber();
        var headerRow = FindBillsHeaderRow(worksheet);

        for (var row = headerRow + 1; row <= lastRow; row++)
        {
            var dueDateCell = worksheet.Cell(row, 1);
            var nameCell = worksheet.Cell(row, 2);
            var amountCell = worksheet.Cell(row, 6);

            if (dueDateCell.IsEmpty() && nameCell.IsEmpty())
            {
                break;
            }

            if (!IsValidBillRow(dueDateCell, nameCell, amountCell))
            {
                continue;
            }

            var dueDay = GetDueDay(dueDateCell, effectiveDate);
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
                    StartDate = GetNextMonthlyOccurrence(effectiveDate, dueDay),
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

    private static int GetDueDay(IXLCell dueDateCell, DateOnly effectiveDate)
    {
        var dueDateText = dueDateCell.GetString().Trim();

        if (dueDateText.Equals("Last day of the month", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month);
        }
        var numericPart = new string([.. dueDateText.TakeWhile(char.IsDigit)]);
        if (!int.TryParse(numericPart, out var dueDay) || dueDay is < 1 or > 31)
        {
            throw new FormatException($"Unable to parse due date '{dueDateText}'.");
        }
        return dueDay;
    }

    private static int FindBillsHeaderRow(IXLWorksheet worksheet)
    {
        var lastRowUsed =
            worksheet.LastRowUsed()
            ?? throw new InvalidOperationException("The worksheet is empty.");
        for (var row = 1; row <= lastRowUsed.RowNumber(); row++)
        {
            var value = worksheet.Cell(row, 1).GetString().Trim();

            if (value.Equals("Payment Due Date", StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        throw new InvalidOperationException("Could not find the monthly bills header row.");
    }
}
