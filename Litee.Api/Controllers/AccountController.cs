using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Litee.Application.Services.Accounts;

namespace Litee.Api.Controllers;

[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
  private readonly IAccountService _accountService = accountService;

  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Accounts.GetAll)]
  public async Task<IActionResult> GetAccounts()
  {
    var accounts = await _accountService.GetAccountsAsync();

    return Ok(accounts);
  }
}
