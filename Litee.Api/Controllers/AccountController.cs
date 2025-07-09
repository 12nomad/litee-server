using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Litee.Application.Services.Accounts;
using Litee.Contracts.Accounts;
using System.Net;
using Litee.Domain.Entities;
using Litee.Contracts.Transactions;
using Litee.Contracts.Authentication.Common;

namespace Litee.Api.Controllers;

[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
  private readonly IAccountService _accountService = accountService;

  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Accounts.GetAll)]
  public async Task<ActionResult<List<Account>>> GetAccounts()
  {
    var result = await _accountService.GetAccountsAsync();
    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Accounts.GetOne)]
  public async Task<ActionResult<PaginationResponse<List<Transaction>, Account>>> GetAccount([FromRoute] int id, [FromQuery] TransactionsPaginationAndFilteringRequest request)
  {
    var result = await _accountService.GetAccountAsync(id, request);

    if (!result.IsSuccess)
      return NotFound(result.Message);

    return Ok(new PaginationResponse<List<Transaction>, Account>()
    {
      CurrentPage = request.Page,
      PageSize = request.PageSize,
      TotalCount = result.Count,
      Data = result.Data,
      Extra = result.Extra
    });
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPost(Routes.Accounts.Create)]
  public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountRequest request)
  {
    var result = await _accountService.CreateAccountAsync(request);

    if (!result.IsSuccess)
      return BadRequest(result.Message);

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPut(Routes.Accounts.Update)]
  public async Task<ActionResult<Account>> UpdateAccount([FromRoute] int id, [FromBody] UpdateAccountRequest request)
  {
    var result = await _accountService.UpdateAccountAsync(id, request);

    if (!result.IsSuccess)
    {
      if (result.ErrorCode == HttpStatusCode.NotFound)
        return NotFound(result.Message);
      return BadRequest(result.Message);
    }

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpDelete(Routes.Accounts.Delete)]
  public async Task<ActionResult<Account>> DeleteAccount([FromRoute] int id)
  {
    var result = await _accountService.DeleteAccountAsync(id);

    if (!result.IsSuccess)
      return NotFound(result.Message);

    return Ok(result.Data);
  }
}
