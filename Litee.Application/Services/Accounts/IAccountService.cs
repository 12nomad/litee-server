using Litee.Domain.Entities;

namespace Litee.Application.Services.Accounts;

public interface IAccountService
{
  Task<IEnumerable<Account>> GetAccountsAsync();
  Task<Account> GetAccountAsync(Guid id);
  Task<Account> CreateAccountAsync(Account account);
  Task<Account> UpdateAccountAsync(Guid id, Account account);
  Task<bool> DeleteAccountAsync(Guid id);
}
