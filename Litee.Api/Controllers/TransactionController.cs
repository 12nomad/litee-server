using Litee.Application.Services.Transactions;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class TransactionController(TransactionService _transactionService) : ControllerBase
{
  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Transactions.GetAll)]
  public async Task<ActionResult<List<Transaction>>> GetAccounts()
  {
    var result = await _transactionService.GetTransactionsAsync();
    return Ok(result.Data);
  }

}
