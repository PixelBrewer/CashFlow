namespace CashFlow.Tests.Services;

using AwesomeAssertions;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Moq;

[TestFixture]
public class ForecastServiceTests
{
    private readonly DateOnly _from = new(2026, 8, 1);
    private readonly DateOnly _through = new(2026, 8, 31);

    private Mock<IRecurringTransactionService> recurringTransactionServiceMock = null!;
    private Mock<IProjectionService> projectionServiceMock = null!;
    private ForecastService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        recurringTransactionServiceMock = new Mock<IRecurringTransactionService>();
        projectionServiceMock = new Mock<IProjectionService>();

        _sut = new ForecastService(
            recurringTransactionServiceMock.Object,
            projectionServiceMock.Object
        );
    }

    [Test]
    public void GenerateForecast_ShouldCombineScheduledAndRecurringTransactions()
    {
        var scheduledTransaction = CreateScheduledTransaction(
            "Car Repair",
            450m,
            TransactionType.Expense,
            new DateOnly(2026, 8, 19)
        );

        var recurringTransaction = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 8, 3)
        );

        var generatedRent = CreateScheduledTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            new DateOnly(2026, 8, 3)
        );

        recurringTransactionServiceMock
            .Setup(x => x.Generate(recurringTransaction, _from, _through))
            .Returns([generatedRent]);

        _sut.GenerateForecast(
            1000m,
            [scheduledTransaction],
            [recurringTransaction],
            _from,
            _through
        );

        projectionServiceMock.Verify(
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
        var expectedProjection = new Projection
        {
            OpeningBalance = 1000m,
            EndingBalance = 1500m,
            LowestBalance = 900m,
            Entries = [],
        };

        projectionServiceMock
            .Setup(x => x.GenerateProjection(1000m, It.IsAny<IEnumerable<ScheduledTransaction>>()))
            .Returns(expectedProjection);

        var result = _sut.GenerateForecast(1000m, [], [], _from, _through);

        result.Should().BeSameAs(expectedProjection);
    }

    [Test]
    public void GenerateForecast_ShouldGenerateTransactionsForEachRecurringTransaction()
    {
        var rent = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 8, 3)
        );

        var paycheck = CreateRecurringTransaction(
            "Paycheck",
            2500m,
            TransactionType.Income,
            RecurrenceFrequency.Biweekly,
            new DateOnly(2026, 8, 7)
        );

        var generatedRent = CreateScheduledTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            new DateOnly(2026, 8, 3)
        );

        var generatedPaycheck = CreateScheduledTransaction(
            "Paycheck",
            2500m,
            TransactionType.Income,
            new DateOnly(2026, 8, 7)
        );

        recurringTransactionServiceMock
            .Setup(x => x.Generate(rent, _from, _through))
            .Returns([generatedRent]);

        recurringTransactionServiceMock
            .Setup(x => x.Generate(paycheck, _from, _through))
            .Returns([generatedPaycheck]);

        _sut.GenerateForecast(
            1000m,
            [],
            [rent, paycheck],
            _from,
            _through
        );

        recurringTransactionServiceMock.Verify(
            x => x.Generate(rent, _from, _through),
            Times.Once
        );

        recurringTransactionServiceMock.Verify(
            x => x.Generate(paycheck, _from, _through),
            Times.Once
        );

        projectionServiceMock.Verify(
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
        var beforeRange = CreateScheduledTransaction(
            "July Expense",
            100m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 31)
        );

        var onStartDate = CreateScheduledTransaction(
            "Start Date Expense",
            200m,
            TransactionType.Expense,
            _from
        );

        var insideRange = CreateScheduledTransaction(
            "August Expense",
            300m,
            TransactionType.Expense,
            new DateOnly(2026, 8, 15)
        );

        var onEndDate = CreateScheduledTransaction(
            "End Date Expense",
            400m,
            TransactionType.Expense,
            _through
        );

        var afterRange = CreateScheduledTransaction(
            "September Expense",
            500m,
            TransactionType.Expense,
            new DateOnly(2026, 9, 1)
        );

        _sut.GenerateForecast(
            1000m,
            [beforeRange, onStartDate, insideRange, onEndDate, afterRange],
            [],
            _from,
            _through
        );

        projectionServiceMock.Verify(
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

    [Test]
    public void GenerateForecast_ShouldThrow_WhenScheduledTransactionsIsNull()
    {
        var act = () => _sut.GenerateForecast(1000m, null!, [], _from, _through);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GenerateForecast_ShouldThrow_WhenRecurringTransactionsIsNull()
    {
        var act = () => _sut.GenerateForecast(1000m, [], null!, _from, _through);

        act.Should().Throw<ArgumentNullException>();
    }

    private static ScheduledTransaction CreateScheduledTransaction(
        string description,
        decimal amount,
        TransactionType type,
        DateOnly date
    )
    {
        return new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            Type = type,
            Date = date,
        };
    }

    private static RecurringTransaction CreateRecurringTransaction(
        string description,
        decimal amount,
        TransactionType type,
        RecurrenceFrequency frequency,
        DateOnly startDate
    )
    {
        return new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            Type = type,
            Frequency = frequency,
            StartDate = startDate,
        };
    }
}
