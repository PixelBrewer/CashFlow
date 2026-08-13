namespace CashFlow.Tests.Api;

using System.Net.Http.Json;
using AwesomeAssertions;
using CashFlow.Api;
using CashFlow.Api.Models.Requests;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;

[TestFixture]
public class ForecastsApiTests
{
    [Test]
    public async Task GenerateForecast_ShouldReturnExpectedProjection()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var request = new CashFlowForecastRequest
        {
            OpeningBalance = 3200m,
            From = new DateOnly(2026, 8, 1),
            Through = new DateOnly(2026, 8, 31),

            ScheduledTransactions =
            [
                new ScheduledTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = "Reimbursement",
                    Date = new DateOnly(2026, 8, 1),
                    Amount = 115m,
                    Type = TransactionType.Income,
                },
                new ScheduledTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = "Car Repair",
                    Date = new DateOnly(2026, 8, 19),
                    Amount = 450m,
                    Type = TransactionType.Expense,
                },
            ],

            RecurringTransactions =
            [
                new RecurringTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = "Rent",
                    Amount = 1600m,
                    Type = TransactionType.Expense,
                    Frequency = RecurrenceFrequency.Monthly,
                    StartDate = new DateOnly(2026, 8, 3),
                },
                new RecurringTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = "Paycheck",
                    Amount = 2500m,
                    Type = TransactionType.Income,
                    Frequency = RecurrenceFrequency.Biweekly,
                    StartDate = new DateOnly(2026, 8, 7),
                },
            ],
        };

        var response = await client.PostAsJsonAsync("/api/forecast", request);

        response.IsSuccessStatusCode.Should().BeTrue();

        var projection = await response.Content.ReadFromJsonAsync<CashFlowProjection>();

        projection.Should().NotBeNull();
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(6265m);
        projection.LowestBalance.Should().Be(1715m);
        projection.Entries.Should().HaveCount(5);
        projection.Entries.Should().NotBeEmpty();
    }
}
