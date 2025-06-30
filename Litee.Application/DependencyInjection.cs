using Litee.Application.Services.Accounts;
using Litee.Application.Services.Authentication;
using Litee.Application.Services.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Litee.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<ITransactionService, TransactionService>();
    return services;
  }
}
