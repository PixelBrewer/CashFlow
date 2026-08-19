namespace CashFlow.Tests.Api;

using System.Net.Http.Json;
using AwesomeAssertions;
using CashFlow.Api;
using CashFlow.Api.Models.Requests;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;

[TestFixture]
public class ForecastApiTests
{
    [Test]
    public async Task GenerateForecast_ShouldReturnExpectedProjection()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var request = new ForecastRequest
        {
            OpeningBalance = 3200m,
            From = new DateOnly(2026, 8, 1),
            Through = new DateOnly(2026, 8, 31),

            ScheduledTransactions =
            [
                CreateScheduledTransaction(
                    "Reimbursement",
                    new DateOnly(2026, 8, 1),
                    115m,
                    TransactionType.Income
                ),
                CreateScheduledTransaction(
                    "Car Repair",
                    new DateOnly(2026, 8, 19),
                    450m,
                    TransactionType.Expense
                ),
            ],

            RecurringTransactions =
            [
                CreateRecurringTransaction(
                    "Rent",
                    1600m,
                    TransactionType.Expense,
                    RecurrenceFrequency.Monthly,
                    new DateOnly(2026, 8, 3)
                ),
                CreateRecurringTransaction(
                    "Paycheck",
                    2500m,
                    TransactionType.Income,
                    RecurrenceFrequency.Biweekly,
                    new DateOnly(2026, 8, 7)
                ),
            ],
        };

        var response = await client.PostAsJsonAsync("/api/forecast", request);

        response.IsSuccessStatusCode.Should().BeTrue();

        var projection = await response.Content.ReadFromJsonAsync<Projection>();

        projection.Should().NotBeNull();
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(6265m);
        projection.LowestBalance.Should().Be(1715m);
        projection.Entries.Should().HaveCount(5);
        projection.Entries.Should().NotBeEmpty();
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
