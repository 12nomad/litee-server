namespace Litee.Api;

public class Routes
{
  private const string BasePath = "/api/v1";

  public static class Authentication
  {
    private const string AuthenticationBasePath = $"{BasePath}/authentication";

    public const string SignUp = AuthenticationBasePath + "/sign-up";
    public const string SignIn = AuthenticationBasePath + "/sign-in";
    public const string GetAuthenticatedUser = AuthenticationBasePath + "/authenticated-user";
    public const string Logout = AuthenticationBasePath + "/logout";
  }

  public static class Accounts
  {
    private const string AccountsBasePath = $"{BasePath}/accounts";

    public const string GetAll = AccountsBasePath;
    public const string GetOne = AccountsBasePath + "/{id}";
    public const string Create = AccountsBasePath;
    public const string Update = AccountsBasePath + "/{id}";
    public const string Delete = AccountsBasePath + "/{id}";
  }

  public static class Transactions
  {
    private const string TransactionsBasePath = $"{BasePath}/transactions";

    public const string GetAll = TransactionsBasePath;
    public const string GetOne = TransactionsBasePath + "/{id}";
    public const string Create = TransactionsBasePath;
    public const string Update = TransactionsBasePath + "/{id}";
    public const string Delete = TransactionsBasePath + "/{id}";
  }
}

