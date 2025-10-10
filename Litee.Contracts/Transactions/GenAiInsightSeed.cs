namespace Litee.Contracts.Transactions;

public class GenAiInsightSeed
{
  public string Description { get; set; } = null!;
  public string Payee { get; set; } = null!;
  public int Amount { get; set; }
  public DateOnly Date { get; set; }
  public string Account { get; set; } = null!;
  public string Category { get; set; } = null!;
}
