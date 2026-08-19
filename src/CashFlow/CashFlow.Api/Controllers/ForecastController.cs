namespace CashFlow.Api.Controllers;

using CashFlow.Api.Models.Requests;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ForecastController(IForecastService forecastService) : ControllerBase
{
    [HttpPost]
    public ActionResult<Projection> Generate(ForecastRequest request)
    {
        var forecast = forecastService.GenerateForecast(
            request.OpeningBalance,
            request.ScheduledTransactions,
            request.RecurringTransactions,
            request.From,
            request.Through
        );
        return Ok(forecast);
    }
}
