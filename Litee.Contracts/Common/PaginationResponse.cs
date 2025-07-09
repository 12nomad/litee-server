namespace Litee.Contracts.Authentication.Common;

public class PaginationResponse<T, V>
{
  public int PageSize { get; set; }
  public int CurrentPage { get; set; }
  public int TotalCount { get; set; }
  public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
  public T? Data { get; set; }
  public V? Extra { get; set; }
}
