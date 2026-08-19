namespace CashFlow.Cli.Settings;

using Spectre.Console.Cli;

public sealed class ImportSettings : CommandSettings
{
    [CommandArgument(0, "[workbook-path]")]
    public string? WorkbookPath { get; init; }
}
