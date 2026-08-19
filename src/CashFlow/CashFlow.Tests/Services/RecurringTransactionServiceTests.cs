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
    public void SetUp()
    {
        _sut = new RecurringTransactionService();
    }

    [Test]
    public void Generate_ShouldCreateMonthlyTransactionsWithinRange()
    {
        var recurringTransaction = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 8, 3)
        );

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 11, 30)
        );

        result
            .Select(transaction => transaction.Date)
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
        var recurringTransaction = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 6, 3)
        );

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 10, 31)
        );

        result
            .Select(transaction => transaction.Date)
            .Should()
            .Equal(
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 9, 3),
                new DateOnly(2026, 10, 3)
            );
    }

    [Test]
    public void Generate_ShouldPreserveEndOfMonthRecurrence()
    {
        var recurringTransaction = CreateRecurringTransaction(
            "Credit Card",
            500m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 1, 31)
        );

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 30)
        );

        result
            .Select(transaction => transaction.Date)
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
        var recurringTransaction = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 12, 3)
        );

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
        var recurringTransaction = CreateRecurringTransaction(
            "Rent",
            1600m,
            TransactionType.Expense,
            RecurrenceFrequency.Monthly,
            new DateOnly(2026, 8, 3)
        );

        var act = () =>
            _sut.Generate(
                recurringTransaction,
                new DateOnly(2026, 11, 30),
                new DateOnly(2026, 8, 1)
            );

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Generate_ShouldCreateWeeklyTransactionsWithinRange()
    {
        var recurringTransaction = CreateRecurringTransaction(
            "Weekly Expense",
            50m,
            TransactionType.Expense,
            RecurrenceFrequency.Weekly,
            new DateOnly(2026, 8, 3)
        );

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 24)
        );

        result
            .Select(transaction => transaction.Date)
            .Should()
            .Equal(
                new DateOnly(2026, 8, 3),
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 17),
                new DateOnly(2026, 8, 24)
            );
    }

    [Test]
    public void Generate_ShouldCreateBiweeklyTransactionsWithinRange()
    {
        var recurringTransaction = CreateRecurringTransaction(
            "Paycheck",
            2500m,
            TransactionType.Income,
            RecurrenceFrequency.Biweekly,
            new DateOnly(2026, 8, 7)
        );

        var result = _sut.Generate(
            recurringTransaction,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 9, 4)
        );

        result
            .Select(transaction => transaction.Date)
            .Should()
            .Equal(
                new DateOnly(2026, 8, 7),
                new DateOnly(2026, 8, 21),
                new DateOnly(2026, 9, 4)
            );
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
