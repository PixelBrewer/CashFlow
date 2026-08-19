namespace CashFlow.Tests.Infrastructure.Excel;

using AwesomeAssertions;
using CashFlow.Core.Enums;
using CashFlow.Infrastructure.Excel;
using ClosedXML.Excel;

[TestFixture]
public class ExcelBudgetProviderTests
{
    private string _filePath = null!;

    [SetUp]
    public void SetUp()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    [Test]
    public void GetBudget_ShouldMapMonthlyBillToRecurringTransaction()
    {
        CreateWorkbook(worksheet =>
            AddBill(worksheet, 3, "20th of the month", "SoFi Personal Loan", 296.82m)
        );

        var provider = new ExcelBudgetProvider(_filePath);

        var effectiveDate = new DateOnly(2026, 8, 13);

        var budget = provider.GetBudget(effectiveDate);

        budget.RecurringTransactions.Should().ContainSingle();

        var transaction = budget.RecurringTransactions.Single();

        transaction.Description.Should().Be("SoFi Personal Loan");
        transaction.Amount.Should().Be(296.82m);
        transaction.Type.Should().Be(TransactionType.Expense);
        transaction.Frequency.Should().Be(RecurrenceFrequency.Monthly);
        transaction.StartDate.Should().Be(new DateOnly(2026, 8, 20));
    }

    [Test]
    public void GetBudget_ShouldUseNextMonth_WhenMonthlyDueDateHasPassed()
    {
        CreateWorkbook(worksheet =>
            AddBill(worksheet, 3, "5th of the month", "SoFi Personal Loan", 296.82m)
        );

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 9, 5));
    }

    [Test]
    public void GetBudget_ShouldMapMultipleMonthlyBills()
    {
        CreateWorkbook(worksheet =>
        {
            AddBill(worksheet, 3, "5th of the month", "SoFi Personal Loan", 296.82m);
            AddBill(worksheet, 4, "15th of the month", "Internet", 70.15m);
            AddBill(worksheet, 5, "20th of the month", "Car Insurance", 120.09m);
        });

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        budget.RecurringTransactions.Should().HaveCount(3);

        budget
            .RecurringTransactions.Should()
            .BeEquivalentTo(
                [
                    new
                    {
                        Description = "SoFi Personal Loan",
                        Amount = 296.82m,
                        StartDate = new DateOnly(2026, 9, 5),
                    },
                    new
                    {
                        Description = "Internet",
                        Amount = 70.15m,
                        StartDate = new DateOnly(2026, 8, 15),
                    },
                    new
                    {
                        Description = "Car Insurance",
                        Amount = 120.09m,
                        StartDate = new DateOnly(2026, 8, 20),
                    },
                ],
                options => options.WithStrictOrdering()
            );
    }

    [Test]
    public void GetBudget_ShouldSkipRowsThatAreNotValidBills()
    {
        CreateWorkbook(worksheet =>
        {
            AddBill(worksheet, 3, "5th of the month", "SoFi Personal Loan", 296.82m);

            worksheet.Cell("A4").Value = "15th of the month";
            worksheet.Cell("B4").Value = "Paused Investment";

            worksheet.Cell("A5").Value = "20th of the month";
            worksheet.Cell("F5").Value = 50m;
        });

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        budget.RecurringTransactions.Should().ContainSingle();

        budget.RecurringTransactions.Single().Description.Should().Be("SoFi Personal Loan");
    }

    [Test]
    public void GetBudget_ShouldStopReadingBills_WhenBlankRowIsReached()
    {
        CreateWorkbook(worksheet =>
        {
            AddBill(worksheet, 3, "5th of the month", "SoFi Personal Loan", 296.82m);

            // End of monthly bill section.
            worksheet.Cell("A4").Value = "";
            worksheet.Cell("B4").Value = "";

            // Represents another section later in the worksheet.
            AddBill(worksheet, 5, "10th of the month", "Should Not Be Imported", 999m);
        });

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        budget.RecurringTransactions.Should().ContainSingle();

        budget.RecurringTransactions.Single().Description.Should().Be("SoFi Personal Loan");
    }

    [Test]
    public void GetBudget_ShouldFindBillsHeaderRow_WhenHeaderIsNotFirstRow()
    {
        CreateWorkbook(worksheet =>
        {
            worksheet.Cell("B1").Value = "Monthly bills and debts";
            AddBill(worksheet, 3, "12th of the month", "Internet", 70.15m);
        });

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        var transaction = budget.RecurringTransactions.Single();

        transaction.Description.Should().Be("Internet");
        transaction.StartDate.Should().Be(new DateOnly(2026, 9, 12));
    }

    [Test]
    public void GetBudget_ShouldMapLastDayOfMonth()
    {
        CreateWorkbook(worksheet =>
            AddBill(worksheet, 3, "Last day of the month", "Monthly Bill", 100m)
        );

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 2, 10));

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Test]
    public void GetBudget_ShouldClampDayOfMonth_WhenMonthIsShorter()
    {
        CreateWorkbook(worksheet =>
            AddBill(worksheet, 3, "31st of the month", "Month End Bill", 50m)
        );

        var provider = new ExcelBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 2, 10));

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 2, 28));
    }

    private static void AddBudgetHeaders(IXLWorksheet worksheet)
    {
        worksheet.Cell("B1").Value = "Monthly bills and debts";

        worksheet.Cell("A2").Value = "Payment Due Date";
        worksheet.Cell("B2").Value = "Name";
        worksheet.Cell("C2").Value = "Loan Interest Rate";
        worksheet.Cell("D2").Value = "Outstanding balance";
        worksheet.Cell("E2").Value = "Minimum Monthly";
        worksheet.Cell("F2").Value = "Actual payment Monthly";
    }

    private void CreateWorkbook(Action<IXLWorksheet> configureWorksheet)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");
        AddBudgetHeaders(worksheet);
        configureWorksheet(worksheet);
        workbook.SaveAs(_filePath);
    }

    private static void AddBill(
        IXLWorksheet worksheet,
        int row,
        string dueDate,
        string name,
        decimal amount
    )
    {
        worksheet.Cell(row, 1).Value = dueDate;
        worksheet.Cell(row, 2).Value = name;
        worksheet.Cell(row, 6).Value = amount;
    }
}
