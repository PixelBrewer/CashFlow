using CashFlow.Infrastructure.Excel;
using Spectre.Console;

var filePath = args.Length > 0 ? args[0] : AnsiConsole.Ask<string>("Path to budget workbook:");

if (string.IsNullOrWhiteSpace(filePath))
{
    AnsiConsole.MarkupLine("[red]Error:[/] A budget workbook path is required.");
    return 1;
}

filePath = Path.GetFullPath(filePath);

if (!File.Exists(filePath))
{
    AnsiConsole.MarkupLine(
        $"[red]Error:[/] Could not find workbook: [yellow]{Markup.Escape(filePath)}[/]"
    );

    return 1;
}

try
{
    var effectiveDate = DateOnly.FromDateTime(DateTime.Today);

    var provider = new ExcelCashFlowBudgetProvider(filePath);

    var budget = provider.GetBudget(effectiveDate);

    var table = new Table().Border(TableBorder.Rounded).Title("[bold]CashFlow Budget Import[/]");

    table.AddColumn("Date");
    table.AddColumn("Description");
    table.AddColumn(new TableColumn("Amount").RightAligned());
    table.AddColumn("Frequency");

    foreach (var transaction in budget.RecurringTransactions)
    {
        table.AddRow(
            transaction.StartDate.ToString("yyyy-MM-dd"),
            Markup.Escape(transaction.Description),
            transaction.Amount.ToString("C"),
            transaction.Frequency.ToString()
        );
    }

    AnsiConsole.Write(table);

    AnsiConsole.MarkupLine(
        $"\n[green]Imported {budget.RecurringTransactions.Count} recurring transactions.[/]"
    );

    return 0;
}
catch (InvalidOperationException exception)
{
    AnsiConsole.MarkupLine($"[red]Unable to import budget:[/] {Markup.Escape(exception.Message)}");

    return 1;
}
catch (FormatException exception)
{
    AnsiConsole.MarkupLine($"[red]Invalid workbook data:[/] {Markup.Escape(exception.Message)}");

    return 1;
}
catch (Exception exception)
{
    AnsiConsole.MarkupLine($"[red]Unexpected error:[/] {Markup.Escape(exception.Message)}");

    return 1;
}
