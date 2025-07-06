using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Accounts;

public class UpdateAccountRequest
{
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Name { get; set; }
}
