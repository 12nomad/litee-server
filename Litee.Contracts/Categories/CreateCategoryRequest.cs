using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Categories;

public class CreateCategoryRequest
{
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Name { get; set; }
}
