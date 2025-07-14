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


  public async Task<PaginatedServicesResult<List<Transaction>, EmptyMetadata>> GetTransactionsAsync(TransactionsPaginationAndFilteringRequest request)
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
      _ => query.OrderByDescending(t => t.Date)
    };

    // * Pagination
    var count = await query.CountAsync();
    var transactions = await query
       .Select(t => new Transaction
       {
         Id = t.Id,
         Description = t.Description,
         Amount = t.Amount,
         Payee = t.Payee,
         Date = t.Date,
         AccountId = t.AccountId,
         UserId = t.UserId,
         CategoryId = t.CategoryId,
         Category = t.Category == null ? null : new Category
         {
           Id = t.Category.Id,
           Name = t.Category.Name
         },
         Account = new Account()
         {
           Id = t.Account.Id,
           Name = t.Account.Name
         }
       })
      .Skip((request.Page - 1) * request.PageSize)
      .Take(request.PageSize)
      .ToListAsync();

    return new PaginatedServicesResult<List<Transaction>, EmptyMetadata>(true, null, null, transactions, count, null);
  }

  public Task<ServicesResult<Transaction>> GetTransactionAsync(int id)
  {
    throw new NotImplementedException();
  }


  public async Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest request)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions.FirstOrDefaultAsync(a => a.UserId == userId && a.AccountId == request.AccountId && a.Description.ToLower() == request.Description.ToLower());

    if (transaction is not null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.BadRequest, "Transaction with the same description already exists", null);

    var newTransaction = new Transaction
    {
      Description = request.Description,
      Amount = request.Amount,
      Payee = request.Payee,
      Date = request.Date,
      AccountId = request.AccountId,
      UserId = userId ?? 0,
      CategoryId = request.CategoryId
    };

    await _databaseContext.Transactions.AddAsync(newTransaction);
    await _databaseContext.SaveChangesAsync();
    return new ServicesResult<Transaction>(true, null, null, newTransaction);
  }

  public async Task<ServicesResult<Transaction>> UpdateTransactionAsync(int id, CreateTransactionRequest request)
  {
    var userId = GetUserId();
    var transaction = await _databaseContext.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (transaction is null)
      return new ServicesResult<Transaction>(false, HttpStatusCode.NotFound, "Transaction not found", null);

    if (transaction.Description.ToLower() != request.Description.ToLower())
    {
      var existingTransaction = await _databaseContext.Transactions.FirstOrDefaultAsync(a => a.UserId == userId && a.AccountId == request.AccountId && a.Description.ToLower() == request.Description.ToLower());
      if (existingTransaction is not null)
        return new ServicesResult<Transaction>(false, HttpStatusCode.BadRequest, "Transaction with this description already exists", null);
    }

    transaction.Description = request.Description;
    transaction.Amount = request.Amount;
    transaction.Payee = request.Payee;
    transaction.Date = request.Date;
    transaction.AccountId = request.AccountId;
    transaction.CategoryId = request.CategoryId;
    await _databaseContext.SaveChangesAsync();
    return new ServicesResult<Transaction>(true, null, null, transaction);
  }

  public async Task<ServicesResult<List<Transaction>>> BulkCreateAsync(BulkCreateTransactionRequest request)
  {
    var userId = GetUserId();
    var transactions = request.Transactions.Select(t => new Transaction
    {
      Description = t.Description,
      Amount = t.Amount,
      Payee = t.Payee,
      Date = t.Date,
      AccountId = t.AccountId,
      UserId = userId ?? 0
    }).ToList();

    await _databaseContext.Transactions.AddRangeAsync(transactions);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<List<Transaction>>(true, null, "Transactions created successfully", null);
  }

  public async Task<ServicesResult<List<Transaction>>> BulkDeleteAsync(BulkDeleteTransactionRequest request)
  {
    var userId = GetUserId();
    var transactions = await _databaseContext.Transactions
      .Where(t => request.TransactionIds!.Contains(t.Id) && t.UserId == userId)
      .Select(t => new Transaction()
      {
        Id = t.Id,
        Description = t.Description,
        Amount = t.Amount,
        Payee = t.Payee,
        Date = t.Date,
        AccountId = t.AccountId,
        UserId = t.UserId,
        Category = t.Category == null ? null : new Category
        {
          Id = t.Category.Id,
          Name = t.Category.Name
        },
        Account = new Account
        {
          Id = t.Account.Id,
          Name = t.Account.Name
        }
      })
      .ToListAsync();

    if (transactions.Count == 0)
      return new ServicesResult<List<Transaction>>(false, HttpStatusCode.NotFound, "No matching transactions found", null);

    _databaseContext.Transactions.RemoveRange(transactions);
    await _databaseContext.SaveChangesAsync();

    return new ServicesResult<List<Transaction>>(true, null, "Transactions deleted successfully", null);
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