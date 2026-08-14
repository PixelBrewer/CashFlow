using System.Reflection;
using CashFlow.Infrastructure.Excel;
using Spectre.Console;

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown";

if (args.Contains("--help") || args.Contains("-h"))
{
    AnsiConsole.MarkupLine("[bold]CashFlow[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine(
        "Import and visualize recurring cash-flow data from a supported Excel workbook."
    );
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine("[bold]Usage[/]");
    AnsiConsole.WriteLine("  cashflow <workbook-path>");
    AnsiConsole.WriteLine("  cashflow");
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine("[bold]Options[/]");
    AnsiConsole.WriteLine("  -h, --help      Show this help information");
    AnsiConsole.WriteLine("      --version   Show the installed CashFlow version");

    return 0;
}

if (args.Contains("--version"))
{
    AnsiConsole.MarkupLine($"CashFlow {version}");
    return 0;
}

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
