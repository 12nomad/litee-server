using Litee.Contracts.Accounts;
using Litee.Contracts.Common;
using Litee.Domain.Entities;

namespace Litee.Application.Services.Accounts;

public interface IAccountService
{
  Task<ServicesResult<List<Account>>> GetAccountsAsync();
  Task<ServicesResult<Account>> GetAccountAsync(int id);
  Task<ServicesResult<Account>> CreateAccountAsync(CreateAccountRequest account);
  Task<ServicesResult<Account>> UpdateAccountAsync(UpdateAccountRequest request);
  Task<ServicesResult<Account>> DeleteAccountAsync(int id);
}
