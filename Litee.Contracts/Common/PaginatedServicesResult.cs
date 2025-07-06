using System.Net;

namespace Litee.Contracts.Common;

public record PaginatedServicesResult<T>
(
  bool IsSuccess,
  HttpStatusCode? ErrorCode = null,
  string? Message = null,
  T? Data = default,
  int Count = default
) : ServicesResult<T>(IsSuccess, ErrorCode, Message, Data);
