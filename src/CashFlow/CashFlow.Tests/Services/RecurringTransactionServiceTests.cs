namespace CashFlow.Tests.Services;

using AwesomeAssertions;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using CashFlow.Core.Services;

[TestFixture]
public class RecurringTransactionServiceTests
{
    private RecurringTransactionService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new RecurringTransactionService();
    }

    [Test]
    public void Generate_ShouldCreateMonthlyTransactionsWithinRange()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 8, 3),
        };

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 11, 30)
        );

        result
            .Select(x => x.Date)
            .Should()
            .Equal(
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 9, 3),
                new DateOnly(2026, 10, 3),
                new DateOnly(2026, 11, 3)
            );
    }

    [Test]
    public void Generate_ShouldSkipTransactionsBeforeRequestRange()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 6, 3),
        };

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 10, 31)
        );

        result
            .Select(x => x.Date)
            .Should()
            .Equal(new DateOnly(2026, 8, 3), new DateOnly(2026, 9, 3), new DateOnly(2026, 10, 3));
    }

    [Test]
    public void Generate_ShouldPreserveEndOfMonthRecurrence()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Credit Card",
            Amount = 500m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 1, 31),
        };

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 30)
        );

        result
            .Select(x => x.Date)
            .Should()
            .Equal(
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 4, 30)
            );
    }

    [Test]
    public void Generate_ShouldReturnEmpty_WhenTransactionStartsAfterRange()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 12, 3),
        };

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 11, 30)
        );

        result.Should().BeEmpty();
    }

    [Test]
    public void Generate_ShouldThrow_WhenFromIsAfterThrough()
    {
        var recurringTransaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Description = "Rent",
            Amount = 1600m,
            Type = TransactionType.Expense,
            Frequency = RecurrenceFrequency.Monthly,
            StartDate = new DateOnly(2026, 8, 3),
        };

        var act = () =>
            _sut.Generate(
                recurringTransaction,
                new DateOnly(2026, 11, 30),
                new DateOnly(2026, 8, 1)
            );

        act.Should().Throw<ArgumentException>();
    }
}
