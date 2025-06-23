using System.Security.Claims;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Litee.Application.Services.Accounts;

public class AccountService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext) : IAccountService
{
  private readonly DatabaseContext _dbContext = dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private const int RetrievedAccountsCount = 3;

  public async Task<IEnumerable<Account>> GetAccountsAsync()
  {
    var accounts = await _dbContext.Accounts
      .Take(RetrievedAccountsCount)
      .Where(a => a.UserId == GetUserId())
      .ToListAsync();
    return accounts;
  }

  public Task<Account> GetAccountAsync(Guid id)
  {
    throw new NotImplementedException();
  }

  public Task<Account> CreateAccountAsync(Account account)
  {
    throw new NotImplementedException();
  }

  public Task<bool> DeleteAccountAsync(Guid id)
  {
    throw new NotImplementedException();
  }

  public Task<Account> UpdateAccountAsync(Guid id, Account account)
  {
    throw new NotImplementedException();
  }

  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}
