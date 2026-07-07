namespace Dento.Constants;

public static class ErrorCodes
{
    // UnHandled Eceptions
    public const string UnhandledException = "UNHANDLED_EXCEPTION";

    // Authentication and Authorization Errors
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string EmailNotVerified = "AUTH_EMAIL_NOT_VERIFIED";
    public const string EmailAlreadyExists = "AUTH_EMAIL_ALREADY_EXISTS";
    public const string UserAlreadyLoggedIn = "AUTH_USER_ALREADY_LOGGED_IN";
    public const string InvalidVerificationCode = "AUTH_INVALID_VERIFICATION_CODE";
    public const string EmailNotFound = "AUTH_EMAIL_NOT_FOUND";
    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string NotOwned = "AUTH_NOT_OWNED";

    // Dentist Schedule
    public const string ScheduleNotFound = "SCHE_SCHEDULE_NOT_FOUND";
}
