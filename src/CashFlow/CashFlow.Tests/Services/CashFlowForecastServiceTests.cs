namespace CashFlow.Tests.Services;

using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Moq;

[TestFixture]
public class CashFlowForecastServiceTests
{
    private Mock<IRecurringTransactionService> recurringTransactionServiceMock = null!;
    private Mock<ICashFlowProjectionService> cashFlowProjectionServiceMock = null!;
    private CashFlowForecastService _sut = null!;

    [SetUp]
    public void Setup()
    {
        recurringTransactionServiceMock = new Mock<IRecurringTransactionService>();
        cashFlowProjectionServiceMock = new Mock<ICashFlowProjectionService>();

        _sut = new CashFlowForecastService(
            recurringTransactionServiceMock.Object,
            cashFlowProjectionServiceMock.Object
        );
    }

    [Test]
    public void GenerateForecast_ShouldCombineScheduledAndRecurringTransactions()
    {
        // Arrange
        var from = new DateOnly(2026, 8, 1);
        var through = new DateOnly(2026, 8, 31);

        var scheduledTransaction = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Car Repair",
            Amount = 450m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 8, 19),
        };

        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 8, 3),
        };

        var generatedRent = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 8, 3),
        };

        recurringTransactionServiceMock
            .Setup(x => x.Generate(recurringTransaction, from, through))
            .Returns([generatedRent]);

        // Act
        _sut.GenerateForecast(1000m, [scheduledTransaction], [recurringTransaction], from, through);

        // Assert
        cashFlowProjectionServiceMock.Verify(
            x =>
                x.GenerateProjection(
                    1000m,
                    It.Is<IEnumerable<ScheduledTransaction>>(transactions =>
                        transactions.Count() == 2
                        && transactions.Contains(scheduledTransaction)
                        && transactions.Contains(generatedRent)
                    )
                ),
            Times.Once
        );
    }
}
