namespace Litee.Contracts.Common;

public record ServicesResult(
    bool IsSuccess,
    string? ErrorCode = null,
    string? Message = null,
    object? Data = null
);
