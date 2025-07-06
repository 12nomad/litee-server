using Litee.Contracts.Authentication.Common;

namespace Litee.Contracts.Transactions;

public class TransactionsPaginationAndFilteringRequest : PaginationRequest
{
  public string? OrderBy { get; set; }
  public string? Search { get; set; }
}
