namespace CashFlow.Tests.Infrastructure.Excel;

using AwesomeAssertions;
using CashFlow.Core.Enums;
using CashFlow.Infrastructure.Excel;
using ClosedXML.Excel;

[TestFixture]
public class ExcelCashFlowBudgetProviderTests
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
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "20th of the month";
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("F2").Value = 296.82m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

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
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "5th of the month";
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("F2").Value = 296.82m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

        var effectiveDate = new DateOnly(2026, 8, 13);

        var budget = provider.GetBudget(effectiveDate);

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 9, 5));
    }

    [Test]
    public void GetBudget_ShouldMapMultipleMonthlyBills()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "5th of the month";
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("F2").Value = 296.82m;

            worksheet.Cell("A3").Value = "15th of the month";
            worksheet.Cell("B3").Value = "Internet";
            worksheet.Cell("F3").Value = 70.15m;

            worksheet.Cell("A4").Value = "20th of the month";
            worksheet.Cell("B4").Value = "Car Insurance";
            worksheet.Cell("F4").Value = 120.09m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

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
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "5th of the month";
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("F2").Value = 296.82m;

            // Completely blank row.
            worksheet.Cell("A3").Value = "";

            // Has a due date and name but no active monthly payment.
            worksheet.Cell("A4").Value = "15th of the month";
            worksheet.Cell("B4").Value = "Paused Investment";

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        budget.RecurringTransactions.Should().ContainSingle();

        budget.RecurringTransactions.Single().Description.Should().Be("SoFi Personal Loan");
    }

    [Test]
    public void GetBudget_ShouldMapLastDayOfMonth()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "Last day of the month";
            worksheet.Cell("B2").Value = "Monthly Bill";
            worksheet.Cell("F2").Value = 100m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 2, 10));

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Test]
    public void GetBudget_ShouldClampDayOfMonth_WhenMonthIsShorter()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");

            AddBudgetHeaders(worksheet);

            worksheet.Cell("A2").Value = "31st of the month";
            worksheet.Cell("B2").Value = "Month End Bill";
            worksheet.Cell("F2").Value = 50m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 2, 10));

        var transaction = budget.RecurringTransactions.Single();

        transaction.StartDate.Should().Be(new DateOnly(2026, 2, 28));
    }

    private static void AddBudgetHeaders(IXLWorksheet worksheet)
    {
        worksheet.Cell("A1").Value = "Payment Due Date";
        worksheet.Cell("B1").Value = "Name";
        worksheet.Cell("C1").Value = "Loan Interest Rate";
        worksheet.Cell("D1").Value = "Outstanding balance";
        worksheet.Cell("E1").Value = "Minimum Monthly";
        worksheet.Cell("F1").Value = "Actual payment Monthly";
    }
}
