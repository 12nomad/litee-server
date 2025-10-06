using Litee.Contracts.Common;
using Litee.Contracts.Reports;

namespace Litee.Application.Services.Reports;

public interface IReportsService
{
  Task<ServicesResult<FinanceResult>> GetReportsAsync(string? from, string? to, int? accountId);
}
