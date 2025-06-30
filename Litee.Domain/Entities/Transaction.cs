using System.ComponentModel.DataAnnotations;

namespace Litee.Domain.Entities;

public class Transaction
{
  public int Id { get; set; }

  [MaxLength(100), MinLength(10)]
  public string Description { get; set; } = null!;

  public decimal Amount { get; set; }

  // navigation properties
  public int AccountId { get; set; }
  public Account Account { get; set; } = null!;
  public int UserId { get; set; }
  public User User { get; set; } = null!;
}
