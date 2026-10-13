namespace MultiGuide.Domain.Constants;

/// <summary>
/// Stable error codes returned in ProblemDetails.code for all business errors.
/// Front-end must use these codes to branch logic, NOT HTTP status codes.
/// </summary>
public static class ErrorCodes
{
    // 401 – missing token
    public const string PassMissing = "PASS_MISSING";
    public const string AuthRequired = "AUTH_REQUIRED";

    // 402 – payment
    public const string PaymentFailed = "PAYMENT_FAILED";

    // 403 – token present but invalid
    public const string PassInvalid = "PASS_INVALID";
    public const string PassExpired = "PASS_EXPIRED";
    public const string PassAreaMismatch = "PASS_AREA_MISMATCH";
    public const string Forbidden = "FORBIDDEN";

    // 404 – not found
    public const string AreaNotFound = "AREA_NOT_FOUND";
    public const string PoiContentNotFound = "POI_CONTENT_NOT_FOUND";
    public const string SessionNotFound = "SESSION_NOT_FOUND";

    // 409 – conflict
    public const string TranslationExists = "TRANSLATION_EXISTS";

    // 422 – validation
    public const string ValidationError = "VALIDATION_ERROR";
    public const string IdempotencyKeyReused = "IDEMPOTENCY_KEY_REUSED";

    // 423 – account locked
    public const string AccountLocked = "ACCOUNT_LOCKED";

    // 429 – rate limited
    public const string RateLimited = "RATE_LIMITED";

    // 500 – internal server error
    public const string InternalError = "INTERNAL_ERROR";
}
