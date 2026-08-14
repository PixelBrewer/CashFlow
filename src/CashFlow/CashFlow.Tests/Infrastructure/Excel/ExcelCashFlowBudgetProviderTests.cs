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
            var worksheet = workbook.AddWorksheet("Budget");

            worksheet.Cell("A1").Value = "Payment Due Date";
            worksheet.Cell("B1").Value = "Name";
            worksheet.Cell("C1").Value = "Actual payment Monthly";

            worksheet.Cell("A2").Value = 20;
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("C2").Value = 296.82m;

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
            var worksheet = workbook.AddWorksheet("Budget");

            worksheet.Cell("A1").Value = "Payment Due Date";
            worksheet.Cell("B1").Value = "Name";
            worksheet.Cell("C1").Value = "Actual payment Monthly";

            worksheet.Cell("A2").Value = 5;
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("C2").Value = 296.82m;

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
            var worksheet = workbook.AddWorksheet("Budget");

            worksheet.Cell("A1").Value = "Payment Due Date";
            worksheet.Cell("B1").Value = "Name";
            worksheet.Cell("C1").Value = "Actual payment Monthly";

            worksheet.Cell("A2").Value = 5;
            worksheet.Cell("B2").Value = "SoFi Personal Loan";
            worksheet.Cell("C2").Value = 296.82m;

            worksheet.Cell("A3").Value = 15;
            worksheet.Cell("B3").Value = "Internet";
            worksheet.Cell("C3").Value = 70.15m;

            worksheet.Cell("A4").Value = 20;
            worksheet.Cell("B4").Value = "Car Insurance";
            worksheet.Cell("C4").Value = 120.09m;

            workbook.SaveAs(_filePath);
        }

        var provider = new ExcelCashFlowBudgetProvider(_filePath);

        var budget = provider.GetBudget(new DateOnly(2026, 8, 13));

        budget.RecurringTransactions.Should().HaveCount(3);

        budget
            .RecurringTransactions.Select(transaction => transaction.Description)
            .Should()
            .BeEquivalentTo("SoFi Personal Loan", "Internet", "Car Insurance");
    }
}
