namespace Litee.Api.Middlewares;

public class ClearJwtOnUnauthorizedMiddleware
{
  private readonly RequestDelegate _next;

  public ClearJwtOnUnauthorizedMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task Invoke(HttpContext context)
  {
    await _next(context);

    if (context.Response.StatusCode == 401)
    {
      context.Response.Cookies.Delete("access_token");
    }
  }

}
