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

        var request = new CashFlowProjectionRequest
        {
            OpeningBalance = 3200m,
            Transactions =
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
                    Description = "Rent",
                    Date = new DateOnly(2026, 8, 3),
                    Amount = 1600m,
                    Type = TransactionType.Expense,
                },
                new ScheduledTransaction
                {
                    Id = Guid.NewGuid(),
                    Description = "Pay Advance",
                    Date = new DateOnly(2026, 8, 12),
                    Amount = 2500m,
                    Type = TransactionType.Income,
                },
            ],
        };

        var response = await client.PostAsJsonAsync("/api/projections", request);

        response.IsSuccessStatusCode.Should().BeTrue();

        var projection = await response.Content.ReadFromJsonAsync<CashFlowProjection>();

        projection.Should().NotBeNull();
        projection.OpeningBalance.Should().Be(3200m);
        projection.EndingBalance.Should().Be(4215m);
        projection.LowestBalance.Should().Be(1715m);
        projection.Entries.Should().HaveCount(3);
    }
}
