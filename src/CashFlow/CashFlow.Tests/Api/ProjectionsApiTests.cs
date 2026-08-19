namespace CashFlow.Tests.Api;

using System.Net.Http.Json;
using AwesomeAssertions;
using CashFlow.Api;
using CashFlow.Api.Models.Requests;
using CashFlow.Core.Enums;
using CashFlow.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;

[TestFixture]
public class ProjectionsApiTests
{
    [Test]
    public async Task GenerateProjection_ShouldReturnExpectedProjection()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var request = new ProjectionRequest
        {
            OpeningBalance = 3200m,
            Transactions =
            [
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
            ],
        };

        var response = await client.PostAsJsonAsync("/api/projections", request);

        response.IsSuccessStatusCode.Should().BeTrue();

        var projection = await response.Content.ReadFromJsonAsync<Projection>();

        projection.Should().NotBeNull();
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(4215m);
        projection.LowestBalance.Should().Be(1715m);
        projection.Entries.Should().HaveCount(3);
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
