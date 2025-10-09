using System.ComponentModel.DataAnnotations;

namespace Litee.Domain.Entities;

public class Transaction
{
  public int Id { get; set; }

  [MaxLength(100), MinLength(10)]
  public string Description { get; set; } = null!;

  public string Payee { get; set; } = null!;
  public int Amount { get; set; }
  public DateOnly Date { get; set; }

  // navigation properties
  public int AccountId { get; set; }
  public Account Account { get; set; } = null!;
  public int UserId { get; set; }
  public User User { get; set; } = null!;
  public int? CategoryId { get; set; }
  public Category? Category { get; set; }
  public int? ReceiptId { get; set; }
  public Receipt? Receipt { get; set; }
}
