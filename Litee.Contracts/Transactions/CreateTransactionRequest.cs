using System.ComponentModel.DataAnnotations;

namespace Litee.Contracts.Transactions;

public class CreateTransactionRequest
{

  [MaxLength(100), MinLength(10)]
  public string Description { get; set; } = null!;

  [Required]
  public decimal Amount { get; set; }

  [Required]
  public int AccountId { get; set; }
}
