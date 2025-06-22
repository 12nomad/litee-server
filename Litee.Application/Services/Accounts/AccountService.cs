using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Litee.Application.Services.Accounts;

public class AccountService(DatabaseContext dbContext) : IAccountService
{
  private readonly DatabaseContext _dbContext = dbContext;
  private const int RetrievedAccountsCount = 3;

  public async Task<IEnumerable<Account>> GetAccountsAsync()
  {
    var accounts = await _dbContext.Accounts
      .Take(RetrievedAccountsCount)
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
}
