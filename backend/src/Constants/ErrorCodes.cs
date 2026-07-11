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


    // Slot Appointment
    public const string SlotIsNotFound = "SLOT_IS_NOT_FOUND";
    public const string SlotIsNotAvailable = "SLOT_IS_NOT_AVAILABLE";
    public const string SlotAppointmentConflict = "SLOT_APPOINTMENT_CONFLICT";
    public const string SlotLockExpired = "SLOT_LOCK_EXPIRED";

    // Appointment
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string AppointmentNotOwned = "APPOINTMENT_NOT_OWNED";

    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";

    // Payment
    public const string PaymentConflict = "PAYMENT_CONFLICT";
    public const string PaymentGatewayError = "PAYMENT_GATEWAY_ERROR";
    public const string PaymentNotFound = "PAYMENT_NOT_FOUND";
    public const string PaymentAlreadyProcessed = "PAYMENT_ALREADY_PROCESSED";
    public const string PaymentWebhookInvalid = "PAYMENT_WEBHOOK_INVALID";
    public const string PaymentVerificationFailed = "PAYMENT_VERIFICATION_FAILED";
    public const string AppointmentPaymentFailed = "APPOINTMENT_PAYMENT_FAILED";
}
