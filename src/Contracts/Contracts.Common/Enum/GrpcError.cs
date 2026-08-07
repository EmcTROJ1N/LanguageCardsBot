namespace Cards.Domain.Enum;

public enum GrpcError
{
    /// <summary>
    /// Not an error; returned on success.
    /// HTTP: 200 OK
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The operation was cancelled.
    /// HTTP: 499 Client Closed Request
    /// </summary>
    Cancelled = 1,

    /// <summary>
    /// Unknown error.
    /// HTTP: 500 Internal Server Error
    /// </summary>
    Unknown = 2,

    /// <summary>
    /// Client specified an invalid argument.
    /// HTTP: 400 Bad Request
    /// </summary>
    InvalidArgument = 3,

    /// <summary>
    /// Deadline expired before operation could complete.
    /// HTTP: 504 Gateway Timeout
    /// </summary>
    DeadlineExceeded = 4,

    /// <summary>
    /// Some requested entity was not found.
    /// HTTP: 404 Not Found
    /// </summary>
    NotFound = 5,

    /// <summary>
    /// Entity already exists.
    /// HTTP: 409 Conflict
    /// </summary>
    AlreadyExists = 6,

    /// <summary>
    /// Caller does not have permission.
    /// HTTP: 403 Forbidden
    /// </summary>
    PermissionDenied = 7,

    /// <summary>
    /// Resource exhausted (quota, rate limit, etc).
    /// HTTP: 429 Too Many Requests
    /// </summary>
    ResourceExhausted = 8,

    /// <summary>
    /// Operation rejected because system is not in required state.
    /// HTTP: 400 Bad Request / 412 Precondition Failed
    /// </summary>
    FailedPrecondition = 9,

    /// <summary>
    /// Operation aborted, typically due to concurrency issue.
    /// HTTP: 409 Conflict
    /// </summary>
    Aborted = 10,

    /// <summary>
    /// Operation attempted past valid range.
    /// HTTP: 400 Bad Request
    /// </summary>
    OutOfRange = 11,

    /// <summary>
    /// Operation is not implemented or supported.
    /// HTTP: 501 Not Implemented
    /// </summary>
    Unimplemented = 12,

    /// <summary>
    /// Internal server error.
    /// HTTP: 500 Internal Server Error
    /// </summary>
    Internal = 13,

    /// <summary>
    /// Service currently unavailable.
    /// HTTP: 503 Service Unavailable
    /// </summary>
    Unavailable = 14,

    /// <summary>
    /// Unrecoverable data loss or corruption.
    /// HTTP: 500 Internal Server Error
    /// </summary>
    DataLoss = 15,

    /// <summary>
    /// Request does not have valid authentication credentials.
    /// HTTP: 401 Unauthorized
    /// </summary>
    Unauthenticated = 16
}
