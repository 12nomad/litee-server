using System.Net;
using System.Security.Claims;
using System.Text;
using Litee.Application.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Litee.Api;

public static class CollectionExtensions
{
  public static IServiceCollection AddCustomCors(this IServiceCollection services)
  {
    services.AddCors(options =>
    {
      options.AddPolicy("CorsPolicy",
        builder => builder
          .WithOrigins("http://localhost:3000")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()
      );
    });
    return services;
  }

  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Security:Issuer"],
            ValidAudience = configuration["Security:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Security:SignToken"]!))
          };

          options.Events = new JwtBearerEvents
          {
            OnMessageReceived = context =>
            {
              context.Request.Cookies.TryGetValue("access_token", out var token);
              if (!string.IsNullOrEmpty(token))
                context.Token = token;
              return Task.CompletedTask;
            },
          };
        });
    return services;
  }

  public static IServiceCollection AddGoogleWithJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    var googleClientId = configuration["Api_Keys:GoogleClientId"]
                                   ?? throw new InvalidOperationException("Client id not found.");
    var googleClientSecret = configuration["Api_Keys:GoogleClientSecret"]
                                   ?? throw new InvalidOperationException("Client secret not found.");
    var redirectUrl = configuration["Urls:RedirectUrl"]
                                    ?? throw new InvalidOperationException("RedirectUrl not found.");

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddCookie().AddGoogle(options =>
      {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = Routes.Authentication.SignInGoogleCallback;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
          OnTicketReceived = async context =>
          {
            if (context.Principal is null)
            {
              context.Response.StatusCode = StatusCodes.Status401Unauthorized;
              context.Response.ContentType = "application/json";
              await context.Response.WriteAsync("{ \"error\": \"Unauthorized\" }");
              context.HandleResponse();
            }

            var authenticationService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            var result = await authenticationService.SignInGoogleCallbackAsync(context.Principal!);
            if (!result.IsSuccess)
            {
              context.Response.StatusCode = StatusCodes.Status400BadRequest;
              context.Response.ContentType = "application/json";
              await context.Response.WriteAsync("{ \"error\": \"Bad request\" }");
              context.HandleResponse();
            }

            authenticationService.SetCookieToken((string)result.Data!, context.HttpContext);
            context.Response.Redirect(redirectUrl);
            context.HandleResponse();
          }
        };
      })
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Security:Issuer"],
            ValidAudience = configuration["Security:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Security:SignToken"]!))
          };

          options.Events = new JwtBearerEvents
          {
            OnMessageReceived = context =>
            {
              context.Request.Cookies.TryGetValue("access_token", out var token);
              if (!string.IsNullOrEmpty(token))
                context.Token = token;
              return Task.CompletedTask;
            },
          };
        });
    return services;
  }
}
