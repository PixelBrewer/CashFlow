namespace CashFlow.Infrastructure.Excel;

using CashFlow.Core.Enums;
using CashFlow.Core.Interfaces;
using CashFlow.Core.Models;
using ClosedXML.Excel;

public class ExcelBudgetProvider(string filePath) : IBudgetProvider
{
    public BudgetDefinition GetBudget(DateOnly effectiveDate)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet("Sheet1");

        return new BudgetDefinition
        {
            RecurringTransactions =
                ReadRecurringTransactions(
                    worksheet,
                    effectiveDate
                ),
        };
    }

    private static IReadOnlyList<RecurringTransaction>
        ReadRecurringTransactions(
            IXLWorksheet worksheet,
            DateOnly effectiveDate
        )
    {
        var lastRowUsed = worksheet.LastRowUsed();

        if (lastRowUsed is null)
        {
            return [];
        }

        var recurringTransactions =
            new List<RecurringTransaction>();

        var headerRow = FindBillsHeaderRow(worksheet);
        var lastRow = lastRowUsed.RowNumber();

        for (var row = headerRow + 1; row <= lastRow; row++)
        {
            var dueDateCell = worksheet.Cell(row, 1);
            var nameCell = worksheet.Cell(row, 2);
            var amountCell = worksheet.Cell(row, 6);

            if (IsEndOfBillSection(dueDateCell, nameCell))
            {
                break;
            }

            if (!IsValidBillRow(
                    dueDateCell,
                    nameCell,
                    amountCell
                ))
            {
                continue;
            }

            recurringTransactions.Add(
                MapBillRow(
                    dueDateCell,
                    nameCell,
                    amountCell,
                    effectiveDate
                )
            );
        }

        return recurringTransactions;
    }

    private static RecurringTransaction MapBillRow(
        IXLCell dueDateCell,
        IXLCell nameCell,
        IXLCell amountCell,
        DateOnly effectiveDate
    )
    {
        var dueDay = GetDueDay(
            dueDateCell,
            effectiveDate
        );

        return new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = nameCell.GetString(),
            Amount = amountCell.GetValue<decimal>(),
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = GetNextMonthlyOccurrence(
                effectiveDate,
                dueDay
            ),
        };
    }

    private static bool IsEndOfBillSection(
        IXLCell dueDateCell,
        IXLCell nameCell
    )
    {
        return dueDateCell.IsEmpty()
            && nameCell.IsEmpty();
    }

    private static bool IsValidBillRow(
        IXLCell dueDateCell,
        IXLCell nameCell,
        IXLCell amountCell
    )
    {
        return !dueDateCell.IsEmpty()
            && !nameCell.IsEmpty()
            && !amountCell.IsEmpty();
    }

    private static DateOnly GetNextMonthlyOccurrence(
        DateOnly effectiveDate,
        int dayOfMonth
    )
    {
        var daysInCurrentMonth = DateTime.DaysInMonth(
            effectiveDate.Year,
            effectiveDate.Month
        );

        var currentMonthDay = Math.Min(
            dayOfMonth,
            daysInCurrentMonth
        );

        var candidate = new DateOnly(
            effectiveDate.Year,
            effectiveDate.Month,
            currentMonthDay
        );

        if (candidate >= effectiveDate)
        {
            return candidate;
        }

        var nextMonth = effectiveDate.AddMonths(1);

        var daysInNextMonth = DateTime.DaysInMonth(
            nextMonth.Year,
            nextMonth.Month
        );

        var nextMonthDay = Math.Min(
            dayOfMonth,
            daysInNextMonth
        );

        return new DateOnly(
            nextMonth.Year,
            nextMonth.Month,
            nextMonthDay
        );
    }

    private static int GetDueDay(
        IXLCell dueDateCell,
        DateOnly effectiveDate
    )
    {
        var dueDateText = dueDateCell
            .GetString()
            .Trim();

        if (dueDateText.Equals(
                "Last day of the month",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return DateTime.DaysInMonth(
                effectiveDate.Year,
                effectiveDate.Month
            );
        }

        var numericPart = new string(
            [.. dueDateText.TakeWhile(char.IsDigit)]
        );

        if (!int.TryParse(numericPart, out var dueDay)
            || dueDay is < 1 or > 31)
        {
            throw new FormatException(
                $"Unable to parse due date '{dueDateText}'."
            );
        }

        return dueDay;
    }

    private static int FindBillsHeaderRow(
        IXLWorksheet worksheet
    )
    {
        var lastRowUsed =
            worksheet.LastRowUsed()
            ?? throw new InvalidOperationException(
                "The worksheet is empty."
            );

        for (
            var row = 1;
            row <= lastRowUsed.RowNumber();
            row++
        )
        {
            var value = worksheet
                .Cell(row, 1)
                .GetString()
                .Trim();

            if (value.Equals(
                    "Payment Due Date",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return row;
            }
        }

        throw new InvalidOperationException(
            "Could not find the monthly bills header row."
        );
    }
}
