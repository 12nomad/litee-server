using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Litee.Contracts.Transactions;

public class BulkDeleteTransactionRequest
{
  public List<int>? TransactionIds { get; set; }
}
