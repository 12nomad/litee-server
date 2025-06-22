using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Litee.Domain;

public static class CollectionExtensions
{
  public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetSection("DefaultConnection")["ConnectionString"];
    services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString, ex => ex.MigrationsAssembly("Litee.Api")));
    return services;
  }
}
