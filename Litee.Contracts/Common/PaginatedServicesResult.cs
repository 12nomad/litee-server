using System.Net;

namespace Litee.Contracts.Common;

public record PaginatedServicesResult<T, V>
(
  bool IsSuccess,
  HttpStatusCode? ErrorCode = null,
  string? Message = null,
  T? Data = default,
  int Count = default,
  V? Extra = default
) : ServicesResult<T>(IsSuccess, ErrorCode, Message, Data);
