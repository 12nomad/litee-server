using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Authentication.SignIn;

public class SignInRequest
{
  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Password { get; set; }
}
