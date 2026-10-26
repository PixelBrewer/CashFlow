# CashFlow — Project Design

## 1. Project Vision

CashFlow is a personal finance forecasting application designed to answer a practical question:

**"Given my current balance, expected income, and upcoming expenses, how much money will I have available on any future date?"**

Traditional budgeting tools often focus on categorizing historical spending.

CashFlow emphasizes forward-looking financial planning by calculating the impact of scheduled and recurring transactions on a running account balance.

### Primary Goals

- Import existing financial data from an Excel workbook.
- Model income and expenses.
- Support recurring financial transactions.
- Forecast account balances over a specified date range.
- Identify periods when account balances may become dangerously low.
- Display projections in a readable timeline.
- Eventually provide interactive visualizations.

### Design Philosophy

- Build useful functionality incrementally.
- Keep financial calculations deterministic and testable.
- Separate business logic from presentation.
- Prioritize correctness over unnecessary abstraction.
- Make the CLI useful before developing the web interface.

---

## 2. Technical Architecture

CashFlow is a C#/.NET solution organized around several projects.

### CashFlow.Core

The domain and application logic layer.

Major concepts include:

**ScheduledTransaction**

Represents an individual financial transaction occurring on a specific date.

**Recurring Transactions**

Represent repeating income or expense obligations that must be expanded into individual scheduled transactions.

**CashFlowEntry**

Represents an entry in a projected financial timeline, including its effect on the running balance.

**CashFlowProjection**

Represents the resulting financial projection, including projected entries and balance information.

**Projection Service**

Calculates running balances from an opening balance and scheduled transactions.

**Forecast Service**

Coordinates the process of generating scheduled transactions over a requested date range and producing a projection.

Exact model names, method signatures, and responsibilities must be verified against the current source code.

### CashFlow.Cli

The initial user-facing application.

Built with Spectre.Console and Spectre.Console.Cli.

Intended to support operations such as importing financial data and generating forecasts.

### CashFlow.Api

The future HTTP interface to CashFlow functionality.

The API should reuse existing business logic rather than reimplement financial calculations.

### CashFlow.Tests

Automated tests covering domain and application behavior.

Uses NUnit and the project's existing assertion and mocking libraries.

---

## 3. Forecasting Domain

### 3.1 Opening Balance

The opening balance is the amount of money available at the beginning of the forecast.

It is provided by the caller.

### 3.2 Transaction Types

Transactions represent either:

- Income, which increases the running balance.
- Expense, which decreases the running balance.

Monetary amounts should use `decimal`.

### 3.3 Recurring Transactions

Recurring transactions describe financial events that repeat according to a defined schedule.

Examples include:

- Biweekly paychecks
- Monthly rent
- Monthly loan payments
- Subscription payments
- Other scheduled obligations

The recurrence logic is responsible for determining occurrence dates within a requested forecast period.

### 3.4 Forecast Date Range

A forecast uses a starting date and ending date.

**Both dates are inclusive.**

For example, a forecast from November 1 through November 30 includes transactions occurring on either boundary.

Invalid date ranges should be rejected.

### 3.5 Running Balance

The projection service calculates balances chronologically.

For each transaction:

- Income increases the balance.
- Expenses decrease the balance.
- The resulting balance is recorded.

The projection should also provide the lowest balance encountered, including the opening balance.

Ordering must be deterministic when multiple transactions share a date.

### 3.6 Forecast Result

The forecast should provide sufficient information to answer:

- What is the projected ending balance?
- What is the lowest projected balance?
- When does the lowest balance occur?
- Which transactions affect the balance?
- Does the projected balance become negative?

Some of these may already be implemented, while others may require extending the current model.

Do not add redundant stored properties when a value can be calculated reliably from existing data.

---

## 4. Current Development Status

This section reflects previous development sessions, not a verified inspection of the latest repository.

### Previously Implemented or Worked On

- Core financial models and transaction types
- Recurrence scheduling logic
- Transaction projection service
- Forecast service and associated tests
- NUnit testing infrastructure
- CLI project using Spectre.Console
- Initial workbook import functionality
- CLI forecast command scaffolding
- Release automation and sample workbook packaging

The most recent development focus was the CLI forecasting workflow.

A forecast branch was used for this work.

### Must Be Verified

Before making changes, inspect:

- Current branch and recent commits
- Forecast command and settings classes
- Workbook path resolution
- Import service and workbook parsing
- Forecast service signatures
- CLI dependency registration
- Existing forecast tests
- Current sample workbook format

Mark functionality as complete only after confirming it exists and works.

---

## 5. Next Development Slice: End-to-End CLI Forecasting

### Objective

Enable a user to provide an existing budget workbook, specify an opening balance and forecast period, and receive a useful chronological cash-flow forecast in the terminal.

The intended workflow is:

**Workbook → Import → Scheduled Transactions → Forecast → Projection → CLI Output**

This is the next major milestone toward using CashFlow with real financial data.

### 5.1 Forecast Command

The CLI should expose a forecast command consistent with the existing Spectre.Console.Cli architecture.

Proposed invocation:

```bash
cashflow forecast ./budget.xlsx \
  --balance 2500 \
  --from 2026-11-01 \
  --through 2026-11-30
```

The command syntax is a proposed interface. Verify existing command arguments and options before modifying them.

### 5.2 Command Settings

The command should obtain:

- Workbook path
- Opening balance
- Forecast start date
- Forecast end date

Use the existing settings class where possible.

Validate that:

- The workbook path is valid.
- The file exists.
- The opening balance can be parsed.
- Dates are valid.
- The end date is not before the start date.

Use appropriate Spectre.Console.Cli validation patterns.

### 5.3 Workbook Import

Reuse the existing workbook import functionality.

Do not duplicate workbook parsing inside the forecast command.

Inspect the workbook schema and determine whether it can already supply the required transaction data.

If workbook import does not yet produce the required domain objects, identify the smallest necessary adapter or mapping change.

### 5.4 Forecast Orchestration

The CLI should delegate to the existing forecast service.

Avoid implementing recurrence calculations or running-balance calculations directly inside the command handler.

The forecast service should coordinate existing domain functionality.

### 5.5 Terminal Output

Use Spectre.Console to display an understandable forecast.

Suggested output:

```text
CashFlow Forecast
November 1–30, 2026

Opening Balance: $2,500.00

Date        Description       Type       Amount      Balance
2026-11-01  Rent              Expense   -$1,500.00   $1,000.00
2026-11-06  Paycheck          Income    +$2,800.00   $3,800.00
2026-11-10  Student Loan      Expense     -$650.00   $3,150.00

Ending Balance: $3,150.00
Lowest Balance: $1,000.00
```

This output is illustrative, not an assertion about the current workbook.

A Spectre.Console table is sufficient for the first implementation.

A visual chart or sophisticated terminal dashboard is not required yet.

### 5.6 Error Handling

The CLI should provide actionable errors for:

- Missing workbook
- Invalid workbook format
- Invalid date range
- Invalid opening balance
- Unsupported or malformed transaction data

Avoid exposing unnecessary stack traces for expected user-input errors.

### 5.7 Automated Tests

Test the behavior of the new integration points.

Prioritize:

1. Valid settings and arguments
2. Invalid date ranges
3. Workbook import integration
4. Forecast service invocation
5. Empty transaction sets
6. Correct projection output
7. Useful failure behavior

Reuse existing unit tests rather than duplicating their coverage.

### 5.8 Acceptance Criteria

The forecasting slice is complete when:

- [ ] A user can invoke the forecast command.
- [ ] The command accepts a workbook path.
- [ ] The command accepts an opening balance.
- [ ] The command accepts inclusive forecast dates.
- [ ] Workbook data is converted into usable financial transactions.
- [ ] Existing forecast services produce the projection.
- [ ] The CLI displays a chronological transaction timeline.
- [ ] The CLI displays opening, ending, and lowest balances.
- [ ] Invalid input produces understandable errors.
- [ ] Automated tests pass.
- [ ] The command works with a representative budget workbook.

---

## 6. Suggested Implementation Sequence

This is a provisional plan. Reconcile it with the actual repository before beginning.

### Step 1 — Repository Assessment

Inspect the existing forecast branch.

Determine what has already been implemented and what remains.

Do not rewrite working functionality.

### Step 2 — Forecast Command Settings

Complete and validate forecast command arguments and options.

### Step 3 — Import Integration

Connect workbook import to the existing forecasting pipeline.

### Step 4 — Forecast Execution

Invoke the forecast service using imported data, opening balance, and inclusive dates.

### Step 5 — CLI Presentation

Display the projection using Spectre.Console.

### Step 6 — Testing and Validation

Run unit tests and perform an end-to-end forecast using a representative workbook.

### Step 7 — Documentation

Update the README with command usage and examples.

---

## 7. Future Roadmap

### Milestone 1 — Functional CLI Forecast

Generate a readable financial forecast from an Excel workbook.

**Current priority.**

### Milestone 2 — Improved Forecasting Experience

Potential enhancements:

- Filtering transactions
- Improved terminal presentation
- Warning indicators for negative balances
- Summary statistics
- Better workbook compatibility

### Milestone 3 — HTTP API

Expose forecasting functionality through CashFlow.Api.

Maintain the existing Core business logic.

### Milestone 4 — Web Dashboard

Develop a frontend, potentially using Angular and TypeScript.

Potential features:

- Interactive balance timeline
- Transaction management
- Forecast date selection
- Financial summaries
- Visual identification of low-balance periods

### Milestone 5 — Expanded Financial Planning

Possible longer-term features:

- Multiple accounts
- Savings goals
- Scenario comparisons
- Debt repayment projections
- Persistent transaction storage

These are potential features, not committed requirements.

---

## 8. Architectural Guardrails

The following principles should guide development:

1. The Core project must not depend on CLI presentation.
2. The API and CLI should reuse the same forecasting logic.
3. Monetary calculations must use decimal arithmetic.
4. Date-range semantics must remain consistent.
5. Forecast results must be deterministic for identical inputs.
6. Workbook parsing must remain separate from financial calculations.
7. Prefer extending existing components over duplicating functionality.
8. Avoid premature database, authentication, or frontend infrastructure.
9. Preserve existing tests and public behavior unless a change is intentional.
10. Document important architectural decisions as the project evolves.

---

## 9. Instructions for the Next Agent Session

When asked to begin the forecasting slice:

1. Read `AGENTS.md` and this document.
2. Inspect the actual repository.
3. Check the active branch and working-tree state.
4. Review the existing forecast-related code.
5. Summarize what is implemented and what remains.
6. Recommend one small next development task.
7. Explain the relevant classes and methods.
8. Wait for approval before substantial implementation.

**The immediate objective is a working, end-to-end CLI forecast using real workbook data, not a complete financial management platform.**
