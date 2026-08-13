namespace CashFlow.Api.Controllers;

using CashFlow.Api.Models.Requests;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ForecastController(ICashFlowForecastService cashFlowForecastService) : ControllerBase
{
    [HttpPost]
    public ActionResult<CashFlowProjection> Generate(CashFlowForecastRequest request)
    {
        var forecast = cashFlowForecastService.GenerateForecast(
            request.OpeningBalance,
            request.ScheduledTransactions,
            request.RecurringTransactions,
            request.From,
            request.Through
        );
        return Ok(forecast);
    }
}
