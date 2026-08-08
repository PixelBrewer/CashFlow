namespace CashFlow.Api.Controllers;

using CashFlow.Api.Models.Requests;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProjectionsController(ICashFlowProjectionService cashFlowProjectionService)
    : ControllerBase
{
    [HttpPost]
    public ActionResult<CashFlowProjection> Generate(CashFlowProjectionRequest request)
    {
        var projection = cashFlowProjectionService.GenerateProjection(
            request.OpeningBalance,
            request.Transactions
        );
        return Ok(projection);
    }
}
