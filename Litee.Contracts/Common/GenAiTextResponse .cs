using System.Text.Json.Serialization;

namespace Litee.Contracts.Common;

public class GenAiTextResponse
{
  [JsonPropertyName("payee")]
  public string? Payee { get; set; }

  [JsonPropertyName("amount")]
  public string? Amount { get; set; }

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("date")]
  public string? Date { get; set; }
}
