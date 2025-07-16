namespace Litee.Contracts.Reports;

public class FinanceSummary
{
  public string? CategoryKey { get; set; }
  public DateOnly? DateKey { get; set; }
  public int? Income { get; set; }
  public int? Expense { get; set; }
  public int? Remain { get; set; }
}

public class FinanceResult : FinanceSummary
{
  public int IncomeDifference { get; set; }
  public int ExpenseDifference { get; set; }
  public int RemainDifference { get; set; }
  public List<FinanceSummary>? CategoriesSummary { get; set; }
  public List<FinanceSummary>? DatesSummary { get; set; }
}
