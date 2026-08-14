using CashFlow.Infrastructure.Excel;
using Spectre.Console;

var filePath = AnsiConsole.Ask<string>("Path to budget workbook:");

var effectiveDate = DateOnly.FromDateTime(DateTime.Today);

var provider = new ExcelCashFlowBudgetProvider(filePath);

var budget = provider.GetBudget(effectiveDate);

var table = new Table();

table.AddColumn("Date");
table.AddColumn("Description");
table.AddColumn("Amount");
table.AddColumn("Frequency");

foreach (var transaction in budget.RecurringTransactions)
{
    table.AddRow(
        transaction.StartDate.ToString("yyyy-MM-dd"),
        transaction.Description,
        transaction.Amount.ToString("C"),
        transaction.Frequency.ToString()
    );
}

AnsiConsole.Write(table);
