using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Authentication.SignIn;
using Litee.Contracts.Authentication.SignUp;
using Litee.Contracts.Common;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Litee.Application.Services.Authentication;

public interface IAuthenticationService
{
  public Task<ServicesResult<string>> SignUpAsync(SignUpRequest request);
  public Task<ServicesResult<string>> SignInAsync(SignInRequest request);
  public ServicesResult<AuthenticationResponse> GetUserFromToken(string token);
  public string GenerateJwtToken(User user);
  public void SetCookieToken(string token, HttpContext httpContext);
  public void ClearCookieToken(HttpContext httpContext);
}
