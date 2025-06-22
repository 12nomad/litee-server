using System.ComponentModel.DataAnnotations;

namespace Litee.Domain.Entities;

public class Account
{
  public int Id { get; set; }

  [MaxLength(50)]
  public string Name { get; set; } = null!;

  // navigation properties
  public int UserId { get; set; }
  public User User { get; set; } = null!;

}
