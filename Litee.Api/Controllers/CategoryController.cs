using System.Net;
using Litee.Application.Services.Categories;
using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Categories;
using Litee.Contracts.Transactions;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
  private readonly ICategoryService _categoryService = categoryService;

  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Categories.GetAll)]
  public async Task<ActionResult<List<Category>>> GetCategories()
  {
    var result = await _categoryService.GetCategoriesAsync();
    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpGet(Routes.Categories.GetOne)]
  public async Task<ActionResult<PaginationResponse<List<Transaction>, Category>>> GetCategory([FromRoute] int id, [FromQuery] TransactionsPaginationAndFilteringRequest request)
  {
    var result = await _categoryService.GetCategoryAsync(id, request);

    if (!result.IsSuccess)
      return NotFound(result.Message);

    return Ok(new PaginationResponse<List<Transaction>, Category>()
    {
      CurrentPage = request.Page,
      PageSize = request.PageSize,
      TotalCount = result.Count,
      Data = result.Data,
      Extra = result.Extra
    });
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPost(Routes.Categories.Create)]
  public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryRequest request)
  {
    var result = await _categoryService.CreateCategoryAsync(request);

    if (!result.IsSuccess)
      return BadRequest(result.Message);

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpPut(Routes.Categories.Update)]
  public async Task<ActionResult<Category>> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryRequest request)
  {
    var result = await _categoryService.UpdateCategoryAsync(id, request);

    if (!result.IsSuccess)
    {
      if (result.ErrorCode == HttpStatusCode.NotFound)
        return NotFound(result.Message);
      return BadRequest(result.Message);
    }

    return Ok(result.Data);
  }

  [Authorize(Roles = "Admin, User")]
  [HttpDelete(Routes.Categories.Delete)]
  public async Task<ActionResult<Category>> DeleteCategory([FromRoute] int id)
  {
    var result = await _categoryService.DeleteCategoryAsync(id);

    if (!result.IsSuccess)
      return NotFound(result.Message);

    return Ok(result.Data);
  }
}
