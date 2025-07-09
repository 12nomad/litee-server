using System.Security.Claims;
using Litee.Contracts.Accounts;
using Litee.Contracts.Common;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Litee.Contracts.Transactions;

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

  public async Task<PaginatedServicesResult<List<Transaction>, Account>> GetAccountAsync(int id, TransactionsPaginationAndFilteringRequest request)
  {
    var userId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    if (account is null)
      return new PaginatedServicesResult<List<Transaction>, Account>(false, HttpStatusCode.NotFound, "Account not found", null, 0);

    var query = _dbContext.Transactions.Where(t => t.AccountId == id && t.UserId == userId).AsQueryable();
    // * Date Filters
    if (request.From.HasValue)
      query = query.Where(t => t.Date >= request.From.Value);
    if (request.To.HasValue)
      query = query.Where(t => t.Date <= request.To.Value);
    // * Sorting
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
      }
    })
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync();


    return new PaginatedServicesResult<List<Transaction>, Account>(true, null, null, transactions, count, account);
  }

  public async Task<ServicesResult<Account>> CreateAccountAsync(CreateAccountRequest request)
  {
    var userId = GetUserId();

    var accounts = await _dbContext.Accounts.Where(a => a.UserId == userId).ToListAsync();
    if (accounts.Count > (RetrievedAccountsCount - 1))
      return new ServicesResult<Account>(false, HttpStatusCode.BadRequest, $"Sorry you are limited to {RetrievedAccountsCount} accounts.", null);

    var account = accounts.FirstOrDefault(a => a.UserId == userId && a.Name.ToLower() == request.Name.ToLower());
    if (account is not null)
      return new ServicesResult<Account>(false, HttpStatusCode.BadRequest, "Account with this name already exists", null);

    var newAccount = new Account
    {
      Name = request.Name,
      UserId = userId ?? 0,
    };

    await _dbContext.Accounts.AddAsync(newAccount);
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Account>(true, null, null, newAccount);
  }

  public async Task<ServicesResult<Account>> UpdateAccountAsync(int id, UpdateAccountRequest request)
  {
    var userId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    if (account is null)
      return new ServicesResult<Account>(false, HttpStatusCode.NotFound, "Account not found", null);

    if (account.Name.ToLower() != request.Name.ToLower())
    {
      var existingAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userId && a.Name.ToLower() == request.Name.ToLower());
      if (existingAccount is not null)
        return new ServicesResult<Account>(false, HttpStatusCode.BadRequest, "Account with this name already exists", null);
    }

    account.Name = request.Name;
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Account>(true, null, null, account);
  }

  public async Task<ServicesResult<Account>> DeleteAccountAsync(int id)
  {
    var userId = GetUserId();
    var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

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
