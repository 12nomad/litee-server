using Litee.Contracts.Authentication.Common;

namespace Litee.Contracts.Transactions;

public class TransactionsPaginationAndFilteringRequest : PaginationRequest
{
  public DateOnly? From { get; set; }
  public DateOnly? To { get; set; }
  public string? Search { get; set; }
  public string? OrderBy { get; set; }
}
