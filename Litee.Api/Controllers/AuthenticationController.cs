using Litee.Application.Services.Authentication;
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
    public IActionResult GetAuthenticatedUser()
    {
        var token = HttpContext.Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return Unauthorized("The token is missing");

        var result = _authenticationService.GetUserFromToken(token);
        if (result.Data is null)
            return Unauthorized("Invalid token");

        return Ok(result.Data);
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
