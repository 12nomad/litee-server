using System.Security.Claims;
using Litee.Contracts.Accounts;
using Litee.Contracts.Common;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Litee.Application.Services.Accounts;

public class AccountService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext) : IAccountService
{
  private readonly DatabaseContext _dbContext = dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private const int RetrievedAccountsCount = 3;

  public async Task<ServicesResult<List<Account>>> GetAccountsAsync()
  {
    var accounts = await _dbContext.Accounts
      .Take(RetrievedAccountsCount)
      .Where(a => a.UserId == GetUserId())
      .ToListAsync();

    return new ServicesResult<List<Account>>(true, null, null, accounts);
  }

  public async Task<ServicesResult<Account>> GetAccountAsync(int id)
  {
    var UserId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

    if (account is null)
      return new ServicesResult<Account>(false, HttpStatusCode.NotFound, "Account not found", null);

    return new ServicesResult<Account>(true, null, null, account);
  }

  public async Task<ServicesResult<Account>> CreateAccountAsync(CreateAccountRequest request)
  {
    var UserId = GetUserId();
    var account = _dbContext.Accounts.FirstOrDefault(a => a.Name.ToLower() == request.Name.ToLower() && a.UserId == UserId);

    if (account is not null)
      return new ServicesResult<Account>(false, HttpStatusCode.BadRequest, "Account with this name already exists", null);

    var newAccount = new Account
    {
      Name = request.Name,
      UserId = UserId ?? 0,
    };

    await _dbContext.Accounts.AddAsync(newAccount);
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Account>(true, null, null, newAccount);
  }

  public async Task<ServicesResult<Account>> UpdateAccountAsync(UpdateAccountRequest request)
  {
    var UserId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == UserId);

    if (account is null)
      return new ServicesResult<Account>(false, HttpStatusCode.NotFound, "Account not found", null);

    if (account.Name.ToLower() != request.Name.ToLower())
    {
      var existingAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Name.ToLower() == request.Name.ToLower() && a.UserId == UserId);
      if (existingAccount is not null)
        return new ServicesResult<Account>(false, HttpStatusCode.BadRequest, "Account with this name already exists", null);
    }

    account.Name = request.Name;
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Account>(true, null, null, account);
  }

  public async Task<ServicesResult<Account>> DeleteAccountAsync(int id)
  {
    var UserId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

    if (account is null)
      return new ServicesResult<Account>(false, HttpStatusCode.NotFound, "Account not found", null);

    _dbContext.Accounts.Remove(account);
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Account>(true, null, null, account);
  }

  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}
