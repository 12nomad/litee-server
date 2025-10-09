using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Litee.Application.Services.Transactions;

public interface ITransactionService
{
  Task<PaginatedServicesResult<List<Transaction>, EmptyMetadata>> GetTransactionsAsync(TransactionsPaginationAndFilteringRequest request);
  Task<ServicesResult<Transaction>> GetTransactionAsync(int id);
  Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest request);
  Task<ServicesResult<Transaction>> UpdateTransactionAsync(int id, CreateTransactionRequest request);
  Task<ServicesResult<Transaction>> DeleteTransactionAsync(int id);
  Task<ServicesResult<List<Transaction>>> BulkDeleteAsync(BulkDeleteTransactionRequest request);
  Task<ServicesResult<List<Transaction>>> BulkCreateAsync(BulkCreateTransactionRequest request);
  Task<ServicesResult<ScanReceiptResponse>> ScanReceiptAsync(IFormFile file);
}
