using System.Net;

namespace Litee.Contracts.Common;

// public record ServicesResult(
//     bool IsSuccess,
//     string? ErrorCode = null,
//     string? Message = null,
//     object? Data = null
// );

public record ServicesResult<T>(
    bool IsSuccess,
    HttpStatusCode? ErrorCode = null,
    string? Message = null,
    T? Data = default
);

