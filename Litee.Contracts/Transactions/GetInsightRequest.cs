namespace Litee.Contracts.Common;

public class GetInsightRequest
{
  public string? From { get; set; }
  public string? To { get; set; }
  public int? CategoryId { get; set; }
  public int? AccountId { get; set; }
}
