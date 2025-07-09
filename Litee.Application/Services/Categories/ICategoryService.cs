using Litee.Contracts.Categories;
using Litee.Contracts.Common;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;

namespace Litee.Application.Services.Categories;

public interface ICategoryService
{
  Task<ServicesResult<List<Category>>> GetCategoriesAsync();
  Task<PaginatedServicesResult<List<Transaction>, Category>> GetCategoryAsync(int id, TransactionsPaginationAndFilteringRequest request);

  Task<ServicesResult<Category>> CreateCategoryAsync(CreateCategoryRequest category);
  // Task<ServicesResult<Category>> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
  Task<ServicesResult<Category>> DeleteCategoryAsync(int id);
}
