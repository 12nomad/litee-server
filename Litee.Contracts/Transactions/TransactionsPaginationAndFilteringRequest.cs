using Litee.Contracts.Authentication.Common;

namespace Litee.Contracts.Transactions;

public class TransactionsPaginationAndFilteringRequest : PaginationRequest
{
  public string? From { get; set; }
  public string? To { get; set; }
  public int? CategoryId { get; set; }
  public int? AccountId { get; set; }
  // public string? Search { get; set; }
  public string? OrderBy { get; set; }
}
