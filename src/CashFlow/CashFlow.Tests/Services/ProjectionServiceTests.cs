namespace CashFlow.Tests.Services;

using AwesomeAssertions;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using CashFlow.Core.Services;

[TestFixture]
public class ProjectionServiceTests
{
    private ProjectionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ProjectionService();
    }

    [Test]
    public void GenerateProjection_ShouldReturnOpeningBalance_WhenNoTransactionsExist()
    {
        // Arrange
        const decimal openingBalance = 1000m;

        // Act
        var projection = _sut.GenerateProjection(openingBalance, []);

        // Assert
        projection.OpeningBalance.Should().Be(1000m);
        projection.EndingBalance.Should().Be(1000m);
        projection.LowestBalance.Should().Be(1000m);
        projection.Entries.Should().BeEmpty();
    }

    [Test]
    public void GenerateProjection_ShouldSubtractExpense()
    {
        // Arrange
        var transactions =
            new[]
            {
                CreateScheduledTransaction(
                    "Rent",
                    new DateOnly(2026, 8, 3),
                    1600m,
                    TransactionType.Expense
                ),
            };

        // Act
        var projection = _sut.GenerateProjection(3000m, transactions);

        // Assert
        projection.EndingBalance.Should().Be(1400m);
        projection.LowestBalance.Should().Be(1400m);
    }

    [Test]
    public void GenerateProjection_ShouldAddIncome()
    {
        // Arrange
        var transactions =
            new[]
            {
                CreateScheduledTransaction(
                    "Paycheck",
                    new DateOnly(2026, 8, 12),
                    2500m,
                    TransactionType.Income
                ),
            };

        // Act
        var projection = _sut.GenerateProjection(1000m, transactions);

        // Assert
        projection.EndingBalance.Should().Be(3500m);
        projection.LowestBalance.Should().Be(1000m);
    }

    [Test]
    public void GenerateProjection_ShouldOrderTransactionsByDate()
    {
        // Arrange
        var transactions =
            new[]
            {
                CreateScheduledTransaction(
                    "Paycheck",
                    new DateOnly(2026, 8, 12),
                    2500m,
                    TransactionType.Income
                ),
                CreateScheduledTransaction(
                    "Rent",
                    new DateOnly(2026, 8, 3),
                    1600m,
                    TransactionType.Expense
                ),
            };

        // Act
        var projection = _sut.GenerateProjection(3000m, transactions);

        // Assert
        projection.Entries.Should().HaveCount(2);
        projection.Entries[0].Transaction.Description.Should().Be("Rent");
        projection.Entries[1].Transaction.Description.Should().Be("Paycheck");
    }

    [Test]
    public void GenerateProjection_ShouldCalculateRunningBalance()
    {
        // Arrange
        var transactions =
            new[]
            {
                CreateScheduledTransaction(
                    "Reimbursement",
                    new DateOnly(2026, 8, 1),
                    115m,
                    TransactionType.Income
                ),
                CreateScheduledTransaction(
                    "Rent",
                    new DateOnly(2026, 8, 3),
                    1600m,
                    TransactionType.Expense
                ),
                CreateScheduledTransaction(
                    "Pay Advance",
                    new DateOnly(2026, 8, 12),
                    2500m,
                    TransactionType.Income
                ),
            };

        // Act
        var projection = _sut.GenerateProjection(3200m, transactions);

        // Assert
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(4215m);
        projection.LowestBalance.Should().Be(1715m);

        projection
            .Entries.Select(entry => entry.BalanceAfterTransaction)
            .Should()
            .Equal(3315m, 1715m, 4215m);
    }

    [Test]
    public void GenerateProjection_ShouldThrow_WhenAmountIsNegative()
    {
        // Arrange
        var transactions =
            new[]
            {
                CreateScheduledTransaction(
                    "Invalid",
                    new DateOnly(2026, 8, 3),
                    -10m,
                    TransactionType.Expense
                ),
            };

        // Act
        Action action = () => _sut.GenerateProjection(1000m, transactions);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    private static ScheduledTransaction CreateScheduledTransaction(
        string description,
        DateOnly date,
        decimal amount,
        TransactionType type
    )
    {
        return new ScheduledTransaction
        {
            Id = Guid.NewGuid(),
            Description = description,
            Date = date,
            Amount = amount,
            Type = type,
        };
    }
}
