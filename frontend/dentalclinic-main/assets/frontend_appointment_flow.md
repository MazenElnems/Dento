# Appointment Booking Flow — Frontend Integration Guide

This document outlines the step-by-step API integration flow for booking an appointment, including all necessary endpoints, expected requests/responses, and the error codes the frontend needs to handle.

## Flow Overview

1. **Patient Discovery**: Fetch a list of available dentists.
2. **Schedule Selection**: Fetch the selected dentist's available time slots.
3. **Booking & Lock**: Book a specific time slot. The slot will be **locked for 10 minutes**.
4. **Payment Initialization**: Within the 10-minute window, request a payment intent for the booked appointment.
5. **Payment Completion**: Redirect the user to Paymob to complete the payment.
6. **Confirmation**: Paymob redirects back to the frontend, and the backend webhook confirms the appointment asynchronously.

> [!IMPORTANT]
> **Authentication**: All endpoints in this flow require a valid JWT `Bearer` token in the `Authorization` header. Most require the user to have the `Patient` role.

---

## Step 1: List Dentists

Fetch the list of dentists to display to the patient.

**Endpoint:** `GET /api/v1/Dentists`
**Auth:** Required (Any authenticated user)

### Response (200 OK)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data retrieved successfully",
  "data": [
    {
      "id": "string",
      "fullName": "string",
      "specialty": "string",
      "consultationFee": 200.0,
      "scheduleId": "string", // Use this in Step 2
      "imageUrl": "string",
      "yearsOfExperience": 5
    }
  ]
}
```

---

## Step 2: Get Dentist Schedule

Fetch the available time slots for the selected dentist.

**Endpoint:** `GET /api/v1/Schedules/{scheduleId}`
**Auth:** Required (Role: `Patient`)

### Path Parameters
- `scheduleId`: Obtained from the dentist object in Step 1.

### Response (200 OK)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data retrieved successfully",
  "data": {
    "timeZone": { ... },
    "schedule": [
      {
        "date": "2024-05-20",
        "slots": [
          {
            "id": "string", // Use this in Step 3
            "from": "10:00:00",
            "to": "11:00:00"
          }
        ]
      }
    ]
  }
}
```

---

## Step 3: Book Appointment (Lock Slot)

Book the selected time slot. This locks the slot for **10 minutes**, during which the patient must complete payment.

**Endpoint:** `POST /api/v1/Appointments`
**Auth:** Required (Role: `Patient`)

### Request Body
```json
{
  "slotId": "string"
}
```

### Response (200 OK)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Appointment booked successfully. Please complete payment within 10 minutes.",
  "data": {
    "id": "string", // Appointment ID - Use this in Steps 4 and 5
    "status": "Pending",
    "slotId": "string",
    "slotStatus": "Locked",
    "lockedUntil": "2024-05-20T10:10:00Z",
    "paymentDeadline": "2024-05-20T10:10:00Z" // Same as lockedUntil
  }
}
```

### Validations & Error Codes
Handle these specific `errorCode` values from the error response:
- `SLOT_IS_NOT_FOUND` (404): The provided Slot ID does not exist.
- `SLOT_IS_NOT_AVAILABLE` (409): The slot is already Locked or Booked by someone else.
- `SLOT_APPOINTMENT_CONFLICT` (409): Concurrency error (someone else booked it at the exact same millisecond).

---

## Step 4: Check Appointment Status (Polling/Verification)

Fetch the full details of an appointment. Useful for verifying status after a redirect from the payment gateway or polling while waiting for a webhook.

**Endpoint:** `GET /api/v1/Appointments/{id}`
**Auth:** Required (Role: `Patient`)

### Path Parameters
- `id`: The Appointment ID from Step 3.

### Response (200 OK)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data retrieved successfully",
  "data": {
    "id": "string",
    "status": "Pending", // Or "Confirmed", "Failed", "Canceled"
    "appointmentType": "Consultation",
    "createdAt": "2024-05-20T10:00:00Z",
    "confirmedAt": null,
    "canceledAt": null,
    "slotId": "string",
    "slotDate": "2024-05-21",
    "slotFrom": "10:00:00",
    "slotTo": "11:00:00",
    "slotStatus": "Locked", // Or "Available", "Booked"
    "slotLockedUntil": "2024-05-20T10:10:00Z",
    "dentistId": "string",
    "dentistName": "string",
    "dentistSpecialty": "string",
    "consultationFee": 200.0,
    "paymentId": null,
    "paymentStatus": null, // Or "Pending", "Paid", "Failed"
    "paymentAmount": null,
    "paymentCurrency": null
  }
}
```

### Validations & Error Codes
- `APPOINTMENT_NOT_FOUND` (404): The appointment does not exist.
- `APPOINTMENT_NOT_OWNED` (403): The patient is trying to view an appointment they didn't create.

---

## Step 5: Create Payment Intent

Initialize the payment with Paymob. **Must be done before `paymentDeadline` expires.**

**Endpoint:** `POST /api/v1/Payments/create-payment-intent`
**Auth:** Required (Role: `Patient`)

### Request Body
```json
{
  "appointmentId": "string", // From Step 3
  "idempotencyKey": "string" // Generate a unique UUID (v4) on the frontend for retry safety
}
```

> [!TIP]
> **Idempotency Key**: Always generate a unique UUID on the frontend when the user clicks "Pay". If the network request fails and the user clicks "Pay" again, pass the **same** `idempotencyKey` to prevent double-charging.

### Response (200 OK)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Payment intent created successfully",
  "data": {
    "appointmentId": "string",
    "clientSecret": "string", // Paymob client secret
    "publicKey": "string" // Paymob public key
  }
}
```

### Redirecting to Paymob
The backend automatically sets a `Location` header in the response, but you can also construct the URL manually if you prefer:
`https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}`

Redirect the user to this URL to complete their card details.

### Validations & Error Codes
Handle these critical errors to guide the user:
- `SLOT_LOCK_EXPIRED` (409): The 10-minute window has passed. The frontend should tell the user: *"Your reservation expired. Please go back and select a new time slot."*
- `APPOINTMENT_PAYMENT_FAILED` (400): Payment is already in-progress or already completed, OR the user doesn't own the appointment.
- `PAYMENT_CONFLICT` (409): The frontend sent an `idempotencyKey` that already exists for a different payment attempt.
- `RESOURCE_NOT_FOUND` (404): Appointment ID is invalid.
- `PAYMENT_GATEWAY_ERROR` (502): Paymob is down or unreachable. Tell the user to try again.

---

## Step 6: Payment Result (Frontend Redirect & Webhook)

After the user pays on Paymob, they are redirected back to your frontend application.
**Configured Redirect URL:** `{ClientHost}/payment/result`

### Frontend Action on Redirect:
1. Extract any query parameters Paymob sends back (e.g., success status).
2. Because webhooks can be slightly delayed, call `GET /api/v1/Appointments/{id}` (Step 4) to verify the current status.
3. If `appointment.status == "Confirmed"`, show a success page.
4. If `appointment.status == "Pending"`, show a loading spinner and poll Step 4 every 3 seconds until it changes to `Confirmed` or `Failed`.
5. If `appointment.status == "Failed"`, show an error and direct them to try booking again.
