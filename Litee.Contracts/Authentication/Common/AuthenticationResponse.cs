namespace Litee.Contracts.Authentication.Common;

public class AuthenticationResponse
{
  public int Id { get; set; }
  public required string Role { get; set; }
  public required string Username { get; set; }
  public required string Email { get; set; }
}
