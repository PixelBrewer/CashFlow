using CashFlow.Core.Services;
using Moq;

namespace CashFlow.Tests.Services;

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
        var forecast = _sut.GenerateForecast(
            1000,
            [],
            [],
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 6, 10)
        );
    }
}
