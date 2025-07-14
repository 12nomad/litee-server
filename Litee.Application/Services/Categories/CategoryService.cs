using System.Net;
using System.Security.Claims;
using Litee.Contracts.Categories;
using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Litee.Application.Services.Categories;

public class CategoryService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext) : ICategoryService
{
  private readonly DatabaseContext _dbContext = dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private const int RetrievedCategoriesCount = 6;


  public async Task<ServicesResult<List<Category>>> GetCategoriesAsync()
  {
    var userId = GetUserId();
    var categories = await _dbContext.Categories.Where(c => c.UserId == userId).Take(RetrievedCategoriesCount).ToListAsync();

    return new ServicesResult<List<Category>>(true, null, null, categories);
  }

  public async Task<PaginatedServicesResult<List<Transaction>, Category>> GetCategoryAsync(int id, TransactionsPaginationAndFilteringRequest request)
  {
    var userId = GetUserId();
    var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    if (category is null)
      return new PaginatedServicesResult<List<Transaction>, Category>(false, HttpStatusCode.NotFound, "Category not found", null, 0);

    var query = _dbContext.Transactions.Where(t => t.CategoryId == id && t.UserId == userId).AsQueryable();
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
      CategoryId = t.CategoryId,
      UserId = t.UserId,
      AccountId = t.AccountId,
      Account = new Account
      {
        Id = t.Account.Id,
        Name = t.Account.Name
      }
    })
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToListAsync();


    return new PaginatedServicesResult<List<Transaction>, Category>(true, null, null, transactions, count, category);
  }

  public async Task<ServicesResult<Category>> CreateCategoryAsync(CreateCategoryRequest request)
  {
    var userId = GetUserId();
    var categories = await _dbContext.Categories.Where(a => a.UserId == userId).ToListAsync();
    if (categories.Count > (RetrievedCategoriesCount - 1))
      return new ServicesResult<Category>(false, HttpStatusCode.BadRequest, $"Sorry you are limited to {RetrievedCategoriesCount} categories.", null);

    var category = categories.FirstOrDefault(a => a.UserId == userId && a.Name.ToLower() == request.Name.ToLower());
    if (category is not null)
      return new ServicesResult<Category>(false, HttpStatusCode.BadRequest, "Category with this name already exists", null);

    var newCategory = new Category
    {
      Name = request.Name,
      UserId = userId ?? 0,
    };

    await _dbContext.Categories.AddAsync(newCategory);
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Category>(true, null, null, newCategory);
  }

  public async Task<ServicesResult<Category>> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
  {
    var userId = GetUserId();
    var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    if (category is null)
      return new ServicesResult<Category>(false, HttpStatusCode.NotFound, "Category not found", null);

    if (category.Name.ToLower() != request.Name.ToLower())
    {
      var existingCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == request.Name.ToLower());
      if (existingCategory is not null)
        return new ServicesResult<Category>(false, HttpStatusCode.BadRequest, "Category with this name already exists", null);
    }

    category.Name = request.Name;
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Category>(true, null, null, category);
  }

  public async Task<ServicesResult<Category>> DeleteCategoryAsync(int id)
  {
    var userId = GetUserId();
    var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    if (category is null)
      return new ServicesResult<Category>(false, HttpStatusCode.NotFound, "Category not found", null);

    _dbContext.Categories.Remove(category);
    await _dbContext.SaveChangesAsync();
    return new ServicesResult<Category>(true, null, null, category);
  }

  // * helpers
  public int? GetUserId()
  {
    var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    return int.TryParse(claim?.Value, out var id) ? id : null;
  }
}
