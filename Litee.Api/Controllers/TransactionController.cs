using Litee.Application.Services.Transactions;
using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class TransactionController(ITransactionService _transactionService) : ControllerBase
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

  [Authorize(Roles = "Admin, User")]
  [HttpPost(Routes.Transactions.Create)]
  public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] CreateTransactionRequest request)
  {
    var result = await _transactionService.CreateTransactionAsync(request);

    if (!result.IsSuccess)
      return BadRequest(result.Message);

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPut(Routes.Transactions.Update)]
  public async Task<ActionResult<Transaction>> UpdateTransaction([FromRoute] int id, [FromBody] CreateTransactionRequest request)
  {
    var result = await _transactionService.UpdateTransactionAsync(id, request);

    if (!result.IsSuccess)
      return BadRequest(result.Message);

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPost(Routes.Transactions.BulkDelete)]
  public async Task<IActionResult> BulkDeleteTransactions([FromBody] BulkDeleteTransactionRequest request)
  {
    if (request.TransactionIds is null || !request.TransactionIds.Any())
      return BadRequest("No transaction IDs provided");
    try
    {

      var result = await _transactionService.BulkDeleteAsync(request);

      if (!result.IsSuccess)
        return BadRequest(result.Message);

      return Ok();
    }
    catch (Exception e)
    {

      throw new Exception(e.Message);
    }
  }

  [Authorize(Roles = "Admin, User")]
  [HttpDelete(Routes.Transactions.Delete)]
  public async Task<IActionResult> DeleteTransaction([FromRoute] int id)
  {
    var result = await _transactionService.DeleteTransactionAsync(id);

    if (!result.IsSuccess)
      return NotFound(result.Message);

    return Ok();
  }
}
