using System.Reflection;
using CashFlow.Cli.Commands;
using Spectre.Console.Cli;

var version =
    Assembly.GetExecutingAssembly()
        .GetName()
        .Version?
        .ToString(3)
    ?? "Unknown";

var app = new CommandApp<ImportCommand>();

app.Configure(config =>
{
    config.SetApplicationName("cashflow");
    config.SetApplicationVersion(version);
});

return app.Run(args);
