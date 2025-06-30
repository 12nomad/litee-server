using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;

namespace Litee.Application.Services.Transactions;

public interface ITransactionService
{
  Task<ServicesResult<List<Transaction>>> GetTransactionsAsync();
  Task<ServicesResult<Transaction>> GetTransactionAsync(int id);
  Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest transaction);
}
