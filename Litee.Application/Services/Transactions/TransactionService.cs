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


  public async Task<ServicesResult<List<Transaction>>> GetTransactionsAsync()
  {
    var transactions = await _databaseContext.Transactions
      .Where(a => a.UserId == GetUserId())
      .ToListAsync();

    return new ServicesResult<List<Transaction>>(true, null, null, transactions);
  }

  public Task<ServicesResult<Transaction>> GetTransactionAsync(int id)
  {
    throw new NotImplementedException();
  }


  public Task<ServicesResult<Transaction>> CreateTransactionAsync(CreateTransactionRequest transaction)
  {
    throw new NotImplementedException();
  }

  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}
