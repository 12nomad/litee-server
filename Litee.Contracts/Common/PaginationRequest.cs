namespace Litee.Contracts.Authentication.Common;

public class PaginationRequest
{
  private const int MaxPageSize = 10;
  public int Page { get; set; } = 1;
  private int _pageSize;
  public int PageSize
  {
    get { return _pageSize; }
    set { if (value > MaxPageSize || value < 1) _pageSize = MaxPageSize; else _pageSize = value; }
  }
}
