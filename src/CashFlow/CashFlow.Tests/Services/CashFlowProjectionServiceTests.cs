namespace CashFlow.Tests.Services;

using AwesomeAssertions;
using Core.Enums;
using Core.Models;
using Core.Services;


[TestFixture]
public class CashFlowProjectionServiceTests
{
    private CashFlowProjectionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CashFlowProjectionService();
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
        var transactions = new[]
        {
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Rent",
                Date = new DateOnly(2026, 8, 3),
                Amount = 1600m,
                Type = TransactionType.Expense
            }
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
        var transactions = new[]
        {
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Paycheck",
                Date = new DateOnly(2026, 8, 12),
                Amount = 2500m,
                Type = TransactionType.Income
            }
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
        var transactions = new[]
        {
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Paycheck",
                Date = new DateOnly(2026, 8, 12),
                Amount = 2500m,
                Type = TransactionType.Income
            },
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Rent",
                Date = new DateOnly(2026, 8, 3),
                Amount = 1600m,
                Type = TransactionType.Expense
            }
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
        var transactions = new[]
        {
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Reimbursement",
                Date = new DateOnly(2026, 8, 1),
                Amount = 115m,
                Type = TransactionType.Income
            },
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Rent",
                Date = new DateOnly(2026, 8, 3),
                Amount = 1600m,
                Type = TransactionType.Expense
            },
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Pay Advance",
                Date = new DateOnly(2026, 8, 12),
                Amount = 2500m,
                Type = TransactionType.Income
            }
        };

        // Act
        var projection = _sut.GenerateProjection(3200m, transactions);

        // Assert
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(4215m);
        projection.LowestBalance.Should().Be(1715m);

        projection.Entries.Select(x => x.BalanceAfterTransaction)
            .Should()
            .Equal(3315m, 1715m, 4215m);
    }

    [Test]
    public void GenerateProjection_ShouldThrow_WhenAmountIsNegative()
    {
        // Arrange
        var transactions = new[]
        {
            new ScheduledTransaction
            {
                Id = Guid.NewGuid(),
                Description = "Invalid",
                Date = new DateOnly(2026, 8, 3),
                Amount = -10m,
                Type = TransactionType.Expense
            }
        };

        // Act
        Action action = () =>
            _sut.GenerateProjection(1000m, transactions);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*negative*");
    }
}