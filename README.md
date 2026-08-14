# CashFlow

<!--toc:start-->

- [CashFlow](#cashflow)
  - [Quick Start](#quick-start)
    - [CLI Options](#cli-options)
  - [Features](#features)
    - [v0.1.0](#v010)
  - [Privacy](#privacy)
  - [Installation](#installation)
  - [Supported Excel Format](#supported-excel-format)
    - [Example](#example)
  - [Running From Source](#running-from-source)
    - [Prerequisites](#prerequisites)
    - [Clone](#clone)
    - [Build](#build)
    - [Run](#run)
  - [Architecture](#architecture)
    - [CashFlow.Core](#cashflowcore)
    - [CashFlow.Infrastructure](#cashflowinfrastructure)
    - [CashFlow.Cli](#cashflowcli)
    - [CashFlow.Api](#cashflowapi)
    - [CashFlow.Tests](#cashflowtests)
  - [Development](#development)
  - [Current Limitations](#current-limitations)
  - [Roadmap](#roadmap)
  - [Technology](#technology)
  - [Contributing](#contributing)
  - [License](#license)

<!--toc:end-->

CashFlow is a local-first personal finance application for importing, organizing, and forecasting cash flow.

The project started from a simple problem: spreadsheets are great for tracking a budget, but they make it difficult to answer questions like:

> **What will my actual cash balance look like over the next few weeks or months?**

CashFlow aims to bridge that gap by turning budget data into structured financial information that can be visualized, projected, and eventually managed without relying on a spreadsheet.

> [!NOTE]
> CashFlow is currently in early development. `v0.1.0` focuses on importing recurring expenses from a supported Excel workbook and displaying them through a terminal interface.

## Quick Start

CashFlow is designed around a simple command-line interface:

```bash
cashflow <workbook-path>
```

For example:

```bash
cashflow ~/Documents/Budget.xlsx
```

CashFlow reads the workbook, imports supported recurring expenses, determines their next occurrence, and displays them in the terminal.

You can also start CashFlow without specifying a workbook:

```bash
cashflow
```

CashFlow will prompt you for the workbook path.

### CLI Options

Display help:

```bash
cashflow --help
```

Display the installed version:

```bash
cashflow --version
```

> [!IMPORTANT]
> Packaged `v0.1.0` binaries are currently being prepared. Until they are published, see [Running From Source](#running-from-source) to try CashFlow.

## Features

### v0.1.0

The first release focuses on establishing the Excel-to-CashFlow import pipeline and command-line experience.

- Import recurring expenses from a supported Excel workbook
- Parse human-readable monthly due dates
- Determine the next occurrence of monthly expenses
- Display imported transactions using a terminal UI
- Accept workbook paths from the command line
- Interactive workbook selection when no path is supplied
- Friendly validation and import errors
- `--help` and `--version` CLI options
- Local-only processing of financial data
- Automated tests covering domain, forecasting, API, and Excel import behavior

Supported monthly due-date formats include:

```text
1st of the month
5th of the month
15th of the month
31st of the month
Last day of the month
```

If a due date has already passed during the current month, CashFlow reports the next occurrence in the following month.

## Privacy

CashFlow is currently completely local-first.

Your Excel workbook is read and processed on your own machine. CashFlow does not upload your financial information to a remote server or external service.

`v0.1.0` requires:

- No CashFlow account
- No cloud connection
- No remote database
- No financial-service credentials

Your budget data stays on your machine.

## Installation

Prebuilt binaries for supported platforms will be available through GitHub Releases beginning with `v0.1.0`.

The initial release is planned to support:

- macOS Apple Silicon
- Linux x64
- Windows x64

Detailed installation instructions will be added alongside the `v0.1.0` release artifacts.

Until then, CashFlow can be built and run directly from source.

## Supported Excel Format

The `v0.1.0` Excel importer expects a monthly bills section containing the following columns:

| Column | Field                  |
| ------ | ---------------------- |
| A      | Payment Due Date       |
| B      | Name                   |
| C      | Loan Interest Rate     |
| D      | Outstanding balance    |
| E      | Minimum Monthly        |
| F      | Actual payment Monthly |

The current importer uses:

- **Payment Due Date** to determine the next transaction date
- **Name** as the transaction description
- **Actual payment Monthly** as the recurring transaction amount

The remaining fields are part of the workbook structure and may support additional financial and debt-management features in future releases.

### Example

| Payment Due Date      | Name                  | Loan Interest Rate | Outstanding balance | Minimum Monthly | Actual payment Monthly |
| --------------------- | --------------------- | -----------------: | ------------------: | --------------: | ---------------------: |
| 1st of the month      | Apartment Rent        |                    |                     |         1500.00 |                1500.00 |
| 5th of the month      | Example Personal Loan |              8.25% |             5200.00 |          175.00 |                 200.00 |
| 15th of the month     | Internet              |                    |                     |           70.00 |                  70.00 |
| Last day of the month | Cloud Storage         |                    |                     |           10.00 |                  10.00 |

A sanitized example workbook is included at:

```text
docs/samples/CashFlow.SampleBudget.xlsx
```

## Running From Source

### Prerequisites

Install the .NET SDK and verify it is available:

```bash
dotnet --version
```

### Clone

```bash
git clone https://github.com/PixelBrewer/CashFlow.git
cd CashFlow
```

### Build

```bash
dotnet build src/CashFlow/CashFlow.slnx
```

### Run

Try CashFlow against the included sample workbook:

```bash
dotnet run \
  --project src/CashFlow/CashFlow.Cli \
  -- docs/samples/CashFlow.SampleBudget.xlsx
```

Or use your own supported workbook:

```bash
dotnet run \
  --project src/CashFlow/CashFlow.Cli \
  -- ~/Documents/Budget.xlsx
```

Running without a path starts the interactive prompt:

```bash
dotnet run --project src/CashFlow/CashFlow.Cli
```

## Architecture

CashFlow separates its domain logic, infrastructure, interfaces, and presentation layers:

```text
CashFlow
├── CashFlow.Core
├── CashFlow.Infrastructure
├── CashFlow.Api
├── CashFlow.Cli
└── CashFlow.Tests
```

### CashFlow.Core

Contains the application's domain models, interfaces, recurrence logic, and forecasting services.

Core is intentionally independent of Excel, databases, HTTP, and user-interface technologies.

### CashFlow.Infrastructure

Contains implementations for external data sources and infrastructure concerns.

The current Excel budget provider uses ClosedXML to translate spreadsheet data into CashFlow domain models.

Keeping this behind a Core interface allows Excel to eventually be supplemented or replaced by other data sources without coupling the rest of the application to spreadsheet parsing.

### CashFlow.Cli

The current user-facing CashFlow application.

It uses Spectre.Console to provide a terminal interface for importing and displaying financial data.

The CLI is intended to grow into a more complete cash-flow dashboard while also remaining useful as a diagnostic and development interface as other clients are introduced.

### CashFlow.Api

Provides CashFlow's HTTP boundary.

The API establishes a path toward additional clients, including a future web application.

### CashFlow.Tests

Contains the NUnit automated test suite covering CashFlow's domain behavior, forecasting services, API behavior, and infrastructure integrations.

## Development

Restore dependencies:

```bash
dotnet restore src/CashFlow/CashFlow.slnx
```

Build the solution:

```bash
dotnet build src/CashFlow/CashFlow.slnx
```

Run the complete test suite:

```bash
dotnet test src/CashFlow/CashFlow.slnx
```

Run the CLI:

```bash
dotnet run --project src/CashFlow/CashFlow.Cli
```

Run against the sample workbook:

```bash
dotnet run \
  --project src/CashFlow/CashFlow.Cli \
  -- docs/samples/CashFlow.SampleBudget.xlsx
```

## Current Limitations

CashFlow is still early in development.

For `v0.1.0`:

- Only the supported Excel workbook structure is understood
- Recurring expense import is the primary supported workflow
- Excel column mappings are currently fixed
- Monthly recurrence is the primary recurrence model
- The CLI displays imported transactions rather than the complete cash-flow forecast
- There is no persistent database
- There is no graphical or web interface
- Workbook schema discovery and configuration are limited
- The Excel importer is intentionally narrow while the domain model and import architecture continue to evolve

## Roadmap

CashFlow is being developed incrementally through small end-to-end vertical slices.

Planned areas of development include:

- Cash-flow timeline and balance forecasting
- Income import and projection
- Richer Spectre.Console dashboards
- Configurable Excel import mappings
- Improved recurrence modeling
- Persistent storage
- Debt and loan tracking
- Historical cash-flow analysis
- Expanded REST API
- Web dashboard
- Reduced reliance on Excel over time

The long-term goal is for CashFlow to evolve from a spreadsheet companion into a standalone personal cash-flow management and forecasting application.

## Technology

CashFlow currently uses:

- C# / .NET
- ASP.NET Core
- Spectre.Console
- ClosedXML
- NUnit
- Moq
- Awesome Assertions

## Contributing

CashFlow is currently an early-stage project, but feedback, bug reports, ideas, and contributions are welcome.

More detailed contribution guidelines will be added as the project matures.

## License

CashFlow is licensed under the [MIT License](LICENSE).
