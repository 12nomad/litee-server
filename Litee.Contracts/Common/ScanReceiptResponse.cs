namespace Litee.Contracts.Common;

public class ScanReceiptResponse : GenAiTextResponse
{
  public string? Base64Image { get; set; }
  public int? ReceiptId { get; set; }
}
