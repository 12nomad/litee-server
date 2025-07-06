using System.Net;
using System.Security.Claims;
using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Litee.Application.Services.Transactions;

public class TransactionService(IHttpContextAccessor httpContextAccessor, DatabaseContext databaseContext) : ITransactionService
{
  private readonly DatabaseContext _databaseContext = databaseContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


  public async Task<PaginatedServicesResult<List<Transaction>>> GetTransactionsAsync(TransactionsPaginationAndFilteringRequest request)
  {
    var query = _databaseContext.Transactions
      .AsQueryable();

    // * Filter
    query.Where(t => t.UserId == GetUserId());
    if (!string.IsNullOrEmpty(request.Search))
      query.Where(t => t.Description.ToLower().Contains(request.Search.Trim().ToLower()));

    // * Sort
    query = request.OrderBy switch
    {
      "amount" => query.OrderBy(t => t.Amount),
      "amountDesc" => query.OrderByDescending(t => t.Amount),
      _ => query.OrderByDescending(t => t.Id)
    };

    // * Pagination
    var count = await query.CountAsync();
    var transactions = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

    return new PaginatedServicesResult<List<Transaction>>(true, null, null, transactions, count);
  }

  public Task<ServicesResult<Transaction>> GetTransactionAsync(int id)
  {
    throw new NotImplementedException();
  }


  public Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest transaction)
  {
    throw new NotImplementedException();
  }

  public async Task<ServicesResult<Transaction>> BulkDeleteAsync(BulkDeleteTransactionRequest request)
  {
    var userId = GetUserId();
    var transactions = await _databaseContext.Transactions
      .Where(t => request.TransactionIds!.Contains(t.Id) && t.UserId == userId)
      .ToListAsync();

    if (transactions.Count == 0)
      return new ServicesResult<Transaction>(false, HttpStatusCode.NotFound, "No matching transactions found", null);

    _databaseContext.Transactions.RemoveRange(transactions);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<Transaction>(true, null, "Transactions deleted successfully", null);
  }

  public async Task<ServicesResult<Transaction>> DeleteTransactionAsync(int id)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions
      .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (transaction is null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.NotFound, "Transaction not found", null);

    _databaseContext.Transactions.Remove(transaction);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<Transaction>(true, null, "Transaction deleted successfully", null);
  }

  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}