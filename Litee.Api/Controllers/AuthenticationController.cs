using System.Security.Claims;
using Litee.Application.Services.Authentication;
using Litee.Contracts.Authentication.Common;
using Litee.Contracts.Authentication.SignIn;
using Litee.Contracts.Authentication.SignUp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litee.Api.Controllers;

[ApiController]
public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    [Authorize(Roles = "Admin, User")]
    [HttpGet(Routes.Authentication.GetAuthenticatedUser)]
    public ActionResult<AuthenticationResponse> GetAuthenticatedUser()
    {
        // var token = HttpContext.Request.Cookies["access_token"];
        // if (string.IsNullOrEmpty(token))
        //     return Unauthorized("The token is missing");

        // var result = _authenticationService.GetUserFromToken(token);
        // if (result.Data is null)
        //     return Unauthorized("Invalid token");

        // return Ok(result.Data);

        // * User from context.Token
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new AuthenticationResponse
        {
            Id = int.Parse(id!),
            Username = username!,
            Email = email!,
            Role = role!
        });
    }

    [HttpPost(Routes.Authentication.SignUp)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var result = await _authenticationService.SignUpAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        _authenticationService.SetCookieToken((string)result.Data!, HttpContext);
        return Ok();
    }

    [HttpPost(Routes.Authentication.SignIn)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        var result = await _authenticationService.SignInAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Message);

        _authenticationService.SetCookieToken((string)result.Data!, HttpContext);
        return Ok();
    }

    [Authorize]
    [HttpPost(Routes.Authentication.Logout)]
    public IActionResult Logout()
    {
        _authenticationService.ClearCookieToken(HttpContext);
        return Ok();
    }
}
