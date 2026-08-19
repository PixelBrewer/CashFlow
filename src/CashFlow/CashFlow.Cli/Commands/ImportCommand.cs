namespace CashFlow.Cli.Commands;

using CashFlow.Cli.Settings;
using CashFlow.Core.Models;
using CashFlow.Infrastructure.Excel;
using Spectre.Console;
using Spectre.Console.Cli;

public sealed class ImportCommand : Command<ImportSettings>
{
    public override int Execute(
        CommandContext context,
        ImportSettings settings,
        CancellationToken cancellationToken
    )
    {
        var filePath = GetWorkbookPath(settings);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            AnsiConsole.MarkupLine(
                "[red]Error:[/] A budget workbook path is required."
            );

            return 1;
        }

        filePath = Path.GetFullPath(filePath);

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine(
                $"[red]Error:[/] Could not find workbook: "
                + $"[yellow]{Markup.Escape(filePath)}[/]"
            );

            return 1;
        }

        return ImportBudget(filePath);
    }

    private static string GetWorkbookPath(
        ImportSettings settings
    )
    {
        if (!string.IsNullOrWhiteSpace(
                settings.WorkbookPath
            ))
        {
            return settings.WorkbookPath;
        }

        return AnsiConsole.Ask<string>(
            "Path to budget workbook:"
        );
    }

    private static int ImportBudget(string filePath)
    {
        try
        {
            var effectiveDate =
                DateOnly.FromDateTime(DateTime.Today);

            var provider =
                new ExcelBudgetProvider(filePath);

            var budget =
                provider.GetBudget(effectiveDate);

            RenderBudget(budget);

            return 0;
        }
        catch (InvalidOperationException exception)
        {
            AnsiConsole.MarkupLine(
                $"[red]Unable to import budget:[/] "
                + Markup.Escape(exception.Message)
            );

            return 1;
        }
        catch (FormatException exception)
        {
            AnsiConsole.MarkupLine(
                $"[red]Invalid workbook data:[/] "
                + Markup.Escape(exception.Message)
            );

            return 1;
        }
        catch (Exception exception)
        {
            AnsiConsole.MarkupLine(
                $"[red]Unexpected error:[/] "
                + Markup.Escape(exception.Message)
            );

            return 1;
        }
    }

    private static void RenderBudget(
        BudgetDefinition budget
    )
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]CashFlow Budget Import[/]");

        table.AddColumn("Date");
        table.AddColumn("Description");
        table.AddColumn(
            new TableColumn("Amount")
                .RightAligned()
        );
        table.AddColumn("Frequency");

        foreach (
            var transaction
            in budget.RecurringTransactions
        )
        {
            table.AddRow(
                transaction.StartDate.ToString(
                    "yyyy-MM-dd"
                ),
                Markup.Escape(
                    transaction.Description
                ),
                transaction.Amount.ToString("C"),
                transaction.Frequency.ToString()
            );
        }

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine(
            $"\n[green]Imported "
            + $"{budget.RecurringTransactions.Count} "
            + "recurring transactions.[/]"
        );
    }
}
