using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Accounts;

public class CreateAccountRequest
{
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Name { get; set; }
}
