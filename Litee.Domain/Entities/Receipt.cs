namespace Litee.Domain.Entities;

public class Receipt
{
  public int Id { get; set; }

  public string Base64Image { get; set; } = null!;

  // navigation properties
  public int TransactionId { get; set; }
  public Transaction Transaction { get; set; } = null!;
}
