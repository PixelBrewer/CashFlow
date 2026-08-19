namespace CashFlow.Api.Controllers;

using CashFlow.Api.Models.Requests;
using CashFlow.Core.Models;
using CashFlow.Core.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProjectionsController(IProjectionService projectionService)
    : ControllerBase
{
    [HttpPost]
    public ActionResult<Projection> Generate(ProjectionRequest request)
    {
        var projection = projectionService.GenerateProjection(
            request.OpeningBalance,
            request.Transactions
        );
        return Ok(projection);
    }
}
