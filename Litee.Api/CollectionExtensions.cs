using System.Text;
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
}
