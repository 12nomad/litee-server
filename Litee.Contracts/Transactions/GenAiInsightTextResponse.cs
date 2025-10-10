using System.Text.Json.Serialization;

namespace Litee.Contracts.Common;

public class GenAiInsightTextResponse
{
  [JsonPropertyName("spendingHabitsSummary")]
  public string SpendingHabitsSummary { get; set; } = null!;

  [JsonPropertyName("budgetaryAlertsAndRisks")]
  public string BudgetaryAlertsAndRisks { get; set; } = null!;

  [JsonPropertyName("incomeAndSavingsPotential")]
  public string IncomeAndSavingsPotential { get; set; } = null!;

  [JsonPropertyName("accountHealthAndCashFlow")]
  public string AccountHealthAndCashFlow { get; set; } = null!;

  [JsonPropertyName("strategicRecommendations")]
  public string StrategicRecommendations { get; set; } = null!;
}
