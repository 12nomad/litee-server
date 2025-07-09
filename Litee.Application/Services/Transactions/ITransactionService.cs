using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;

namespace Litee.Application.Services.Transactions;

public interface ITransactionService
{
  Task<PaginatedServicesResult<List<Transaction>, EmptyMetadata>> GetTransactionsAsync(TransactionsPaginationAndFilteringRequest request);
  Task<ServicesResult<Transaction>> GetTransactionAsync(int id);
  Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest request);
  Task<ServicesResult<Transaction>> UpdateTransactionAsync(int id, CreateTransactionRequest request);
  Task<ServicesResult<Transaction>> DeleteTransactionAsync(int id);
  Task<ServicesResult<Transaction>> BulkDeleteAsync(BulkDeleteTransactionRequest request);
}
