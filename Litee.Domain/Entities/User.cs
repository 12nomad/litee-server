using System.ComponentModel.DataAnnotations;

namespace Litee.Domain.Entities;

public class User
{
  public int Id { get; set; }

  public string Role { get; set; } = null!;

  [MaxLength(50)]
  public string Username { get; set; } = null!;

  [EmailAddress, MaxLength(100)]
  public string Email { get; set; } = null!;

  public string PasswordHash { get; set; } = null!;

  public string? ProfilePicture { get; set; }

  // navigation properties
  public ICollection<Account> Accounts { get; set; } = new List<Account>();
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
