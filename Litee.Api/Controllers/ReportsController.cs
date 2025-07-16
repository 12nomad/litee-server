using Litee.Application.Services.Reports;
using Litee.Contracts.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class ReportsController(IReportsService _reportsService) : ControllerBase
{
  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Resports.Reports)]
  public async Task<ActionResult<FinanceResult>> GetResports([FromQuery] string? from, string? to, int accountId)
  {
    var result = await _reportsService.GetReportsAsync(from, to, accountId);

    if (!result.IsSuccess)
      return BadRequest(result.Message);

    return Ok(result.Data);
  }
}
