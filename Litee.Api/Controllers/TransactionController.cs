using Litee.Application.Services.Transactions;
using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class TransactionController(TransactionService _transactionService) : ControllerBase
{
  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Transactions.GetAll)]
  public async Task<ActionResult<PaginationResponse<List<Transaction>>>> GetAccounts([FromQuery] TransactionsPaginationAndFilteringRequest request)
  {
    var result = await _transactionService.GetTransactionsAsync(request);
    return Ok(new PaginationResponse<List<Transaction>>()
    {
      CurrentPage = request.Page,
      PageSize = request.PageSize,
      TotalCount = result.Count,
      Data = result.Data
    });
  }
}
