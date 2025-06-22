using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Authentication.SignUp;

public class SignUpRequest
{
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Username { get; set; }

  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  [Required]
  [StringLength(50, MinimumLength = 2)]
  public required string Password { get; set; }

  // [Compare("Password", ErrorMessage = "Passwords do not match.")]
  // public required string ConfirmPassword { get; set; }
}
