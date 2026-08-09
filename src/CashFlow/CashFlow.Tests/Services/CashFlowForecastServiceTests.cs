namespace CashFlow.Tests.Services;

using AwesomeAssertions;
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

        _sut.GenerateForecast(1000m, [scheduledTransaction], [recurringTransaction], from, through);

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

    [Test]
    public void GenerateForecast_ShouldReturnProjectionFromProjectionService()
    {
        var from = new DateOnly(2026, 8, 1);
        var through = new DateOnly(2026, 8, 31);

        var expectedProjection = new CashFlowProjection
        {
            OpeningBalance = 1000m,
            EndingBalance = 1500m,
            LowestBalance = 900m,
            Entries = [],
        };

        cashFlowProjectionServiceMock
            .Setup(x => x.GenerateProjection(1000m, It.IsAny<IEnumerable<ScheduledTransaction>>()))
            .Returns(expectedProjection);

        var result = _sut.GenerateForecast(1000m, [], [], from, through);

        result.Should().BeSameAs(expectedProjection);
    }

    [Test]
    public void GenerateForecast_ShouldGenerateTransactionsForEachRecurringTransaction()
    {
        var from = new DateOnly(2026, 8, 1);
        var through = new DateOnly(2026, 8, 31);

        var rent = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 8, 3),
        };

        var paycheck = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Paycheck",
            Amount = 2500m,
            Type = TransactionType.Income,
            Frequency = RecurrenceFrequency.Biweekly,
            StartDate = new DateOnly(2026, 8, 7),
        };

        var generatedRent = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 8, 3),
        };

        var generatedPaycheck = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Paycheck",
            Amount = 2500m,
            Type = TransactionType.Income,
            Date = new DateOnly(2026, 8, 7),
        };

        recurringTransactionServiceMock
            .Setup(x => x.Generate(rent, from, through))
            .Returns([generatedRent]);

        recurringTransactionServiceMock
            .Setup(x => x.Generate(paycheck, from, through))
            .Returns([generatedPaycheck]);

        _sut.GenerateForecast(1000m, [], [rent, paycheck], from, through);

        recurringTransactionServiceMock.Verify(x => x.Generate(rent, from, through), Times.Once);

        recurringTransactionServiceMock.Verify(
            x => x.Generate(paycheck, from, through),
            Times.Once
        );

        cashFlowProjectionServiceMock.Verify(
            x =>
                x.GenerateProjection(
                    1000m,
                    It.Is<IEnumerable<ScheduledTransaction>>(transactions =>
                        transactions.Count() == 2
                        && transactions.Contains(generatedRent)
                        && transactions.Contains(generatedPaycheck)
                    )
                ),
            Times.Once
        );
    }

    [Test]
    public void GenerateForecast_ShouldOnlyIncludeScheduledTransactionsWithinForecastRange()
    {
        var from = new DateOnly(2026, 8, 1);
        var through = new DateOnly(2026, 8, 31);

        var beforeRange = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "July Expense",
            Amount = 100m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 7, 31),
        };

        var onStartDate = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Start Date Expense",
            Amount = 200m,
            Type = TransactionType.Expense,
            Date = from,
        };

        var insideRange = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "August Expense",
            Amount = 300m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 8, 15),
        };

        var onEndDate = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "End Date Expense",
            Amount = 400m,
            Type = TransactionType.Expense,
            Date = through,
        };

        var afterRange = new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = "September Expense",
            Amount = 500m,
            Type = TransactionType.Expense,
            Date = new DateOnly(2026, 9, 1),
        };

        _sut.GenerateForecast(
            1000m,
            [beforeRange, onStartDate, insideRange, onEndDate, afterRange],
            [],
            from,
            through
        );

        cashFlowProjectionServiceMock.Verify(
            x =>
                x.GenerateProjection(
                    1000m,
                    It.Is<IEnumerable<ScheduledTransaction>>(transactions =>
                        transactions.Count() == 3
                        && transactions.Contains(onStartDate)
                        && transactions.Contains(insideRange)
                        && transactions.Contains(onEndDate)
                        && !transactions.Contains(beforeRange)
                        && !transactions.Contains(afterRange)
                    )
                ),
            Times.Once
        );
    }

    [Test]
    public void GenerateForecast_ShouldThrow_WhenFromIsAfterThrough()
    {
        var from = new DateOnly(2026, 9, 1);
        var through = new DateOnly(2026, 8, 1);

        var act = () => _sut.GenerateForecast(1000m, [], [], from, through);

        act.Should().Throw<ArgumentException>();
    }
}
