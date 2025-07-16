using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Litee.Contracts.Transactions;

public class CreateTransaction
{

  [MaxLength(100), MinLength(10)]
  public string Description { get; set; } = null!;

  [Required]
  public int Amount { get; set; }

  [Required]
  public string Payee { get; set; } = null!;

  [Required]
  public DateOnly Date { get; set; }

  [Required]
  public int AccountId { get; set; }
}

public class BulkCreateTransactionRequest
{
  public List<CreateTransaction> Transactions { get; set; } = null!;
}
