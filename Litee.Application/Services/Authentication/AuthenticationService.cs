using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Litee.Application.Enums;
using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Authentication.SignIn;
using Litee.Contracts.Authentication.SignUp;
using Litee.Contracts.Common;
using Litee.Domain;
using Litee.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Litee.Application.Services.Authentication;

public class AuthenticationService(IConfiguration configuration, DatabaseContext dbContext) : IAuthenticationService
{
  private readonly DatabaseContext _dbContext = dbContext;
  private readonly IConfiguration _configuration = configuration;
  private readonly int _tokenExpirationDays = 7;

  public ServicesResult GetUserFromToken(string token)
  {
    var data = new JwtSecurityTokenHandler().ReadJwtToken(token);
    var claims = data.Claims;
    return new ServicesResult(true, null, null, new AuthenticationResponse
    {
      Id = int.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!),
      Username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!,
      Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!,
      Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value!
    });
  }

  public async Task<ServicesResult> SignUpAsync(SignUpRequest request)
  {
    // if (request.Password != request.ConfirmPassword)
    //   return new ServicesResult(false, "400", "Passwords don't match", null);

    var user = await _dbContext.Users
      .FirstOrDefaultAsync(user => user.Email == request.Email);
    if (user is not null)
      return new ServicesResult(false, "400", "This email address is already registered", null);

    var newUser = new User();
    var hashedPassword = new PasswordHasher<User>().HashPassword(newUser, request.Password);
    newUser.Username = request.Username;
    newUser.Email = request.Email;
    newUser.PasswordHash = hashedPassword;
    newUser.Role = Enum.GetName<UserRole>(UserRole.User)!;

    await _dbContext.Users.AddAsync(newUser);
    await _dbContext.SaveChangesAsync();
    var token = GenerateJwtToken(newUser);
    return new ServicesResult(true, null, null, token);
  }

  public async Task<ServicesResult> SignInAsync(SignInRequest request)
  {
    var user = await _dbContext.Users
      .FirstOrDefaultAsync(user => user.Email == request.Email);

    if (user is null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
      return new ServicesResult(false, "400", "Invalid credentials", null);

    var token = GenerateJwtToken(user);
    return new ServicesResult(true, null, null, token);
  }

  public string GenerateJwtToken(User user)
  {
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Security:SignToken"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    var tokenDescriptor = new JwtSecurityToken
    (
      issuer: _configuration["Security:Issuer"],
      audience: _configuration["Security:Audience"],
      claims: claims,
      signingCredentials: credentials,
      expires: DateTime.UtcNow.AddDays(_tokenExpirationDays)
    );
    var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    return token;
  }

  public void SetCookieToken(string token, HttpContext httpContext)
  {
    // ! FIXME: Change this when deploying
    httpContext.Response.Cookies.Append("access_token", token, new CookieOptions
    {
      HttpOnly = true,
      IsEssential = true,
      Secure = true,
      SameSite = SameSiteMode.Lax,
      Expires = DateTime.UtcNow.AddDays(_tokenExpirationDays)
    });
  }

  public void ClearCookieToken(HttpContext httpContext)
  {
    httpContext.Response.Cookies.Delete("access_token");
  }
}
