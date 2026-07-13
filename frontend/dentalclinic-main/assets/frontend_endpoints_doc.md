# Frontend API Integration Guide

This document outlines the recently added and updated endpoints across the `Appointments`, `MedicalRecords`, `Patients`, and `Payments` controllers. It includes the expected request bodies, authorization roles, and validation rules required for the frontend integration.

---

## 1. Appointments API

### `POST /api/v1/Appointments`
- **Description:** Books a time slot for the authenticated patient. Behavior changes based on `paymentType`. Online locks the slot for 10 minutes until payment is completed; Cash immediately confirms the appointment.
- **Authorization:** `Patient`
- **Request Body:**
  ```json
  {
      "slotId": "string (Required)",
      "paymentType": "string (Cash or Online - Default is Online)"
  }
  ```
- **Validations / Errors:**
  - `slotId` is required.
  - Returns **404 Not Found** if the slot doesn't exist.
  - Returns **409 Conflict** if the slot is not available.

### `GET /api/v1/Appointments/{id}`
- **Description:** Fetches the full details and current status of an appointment, including slot, dentist, and payment status.
- **Authorization:** `Patient`
- **Path Parameters:**
  - `id`: Appointment ID (string)
- **Validations:**
  - Patients can **only** view their own appointments. Returns **403 Forbidden** if accessed by another user.

---

## 2. Payments API

### `POST /api/v1/Payments/create-payment`
- **Description:** Creates a payment for a booked appointment. For online, returns a client secret and public key for Paymob checkout. For cash, confirms the appointment and marks payment as pending.
- **Authorization:** `Patient`
- **Request Body:**
  ```json
  {
      "appointmentId": "string (Required)",
      "idempotencyKey": "string (Required)",
      "paymentType": "string (Cash or Online - Default is Online)"
  }
  ```
- **Validations:**
  - `appointmentId` is strictly required.
  - `idempotencyKey` is strictly required.

### `POST /api/v1/Payments/{paymentId}/confirm-cash`
- **Description:** Confirms a cash payment after the patient has paid at the clinic.
- **Authorization:** `Receptionist` (Only receptionists can confirm cash payments)
- **Path Parameters:**
  - `paymentId`: The ID of the cash payment to confirm.
- **Validations:**
  - Returns **400 Bad Request** if the payment method is not Cash.
  - Returns **400 Bad Request** if the payment has already been confirmed.
  - Returns **400 Bad Request** if the payment status is not Pending.
  - Returns **404 Not Found** if the payment does not exist.

---

## 3. Medical Records API

### `GET /api/v1/MedicalRecords/{patientId}`
- **Description:** Gets the medical record of a patient, including their past visits and history.
- **Authorization:** `Dentist`, `Patient`, `Admin`
- **Validations:**
  - If the authenticated user is a `Patient`, they can **only** fetch their own `patientId`. Returns **403 Forbidden** otherwise.

### `PUT /api/v1/MedicalRecords/{patientId}/history`
- **Description:** Updates the medical history questionnaire for a patient.
- **Authorization:** `Patient`, `Dentist`
- **Path Parameters:**
  - `patientId`: The ID of the patient.
- **Request Body:**
  ```json
  {
      "medicalConditions": ["string"],
      "allergies": ["string"],
      "pregnancyStatus": "string (NotApplicable, Pregnant, NotPregnant, or PreferNotToSay)",
      "smokingStatus": "string (Never, Former, or Current)",
      "bleedingDisorders": "boolean",
      "heartConditions": "boolean",
      "diabetes": "boolean",
      "highBloodPressure": "boolean",
      "medicalNotes": "string (Optional)"
  }
  ```
- **Validations:**
  - If the authenticated user is a `Patient`, they can **only** update their own medical history. Returns **403 Forbidden** otherwise.

### `POST /api/v1/MedicalRecords/{patientId}/visits`
- **Description:** Adds a visit record (diagnosis, prescriptions, procedures) to a patient's medical record.
- **Authorization:** `Dentist`
- **Path Parameters:**
  - `patientId`: The ID of the patient.
- **Request Body:**
  ```json
  {
      "appointmentId": "string (Required)",
      "diagnosis": "string (Optional)",
      "prescriptions": [
          {
              "medicationName": "string (Required)",
              "dosage": "string (Required)",
              "notes": "string (Optional)"
          }
      ],
      "procedures": [
          {
              "name": "string (Required)",
              "description": "string (Optional)"
          }
      ]
  }
  ```
- **Validations:**
  - `appointmentId` is strictly required.
  - For each prescription: `medicationName` and `dosage` are required.
  - For each procedure: `name` is required.

### `PUT /api/v1/MedicalRecords/{patientId}/visits/{visitRecordId}`
- **Description:** Updates an existing visit record.
- **Authorization:** `Dentist`
- **Path Parameters:**
  - `patientId`: The ID of the patient.
  - `visitRecordId`: The ID of the visit record to update.
- **Request Body:**
  ```json
  {
      "diagnosis": "string (Optional)",
      "prescriptions": [
          {
              "medicationName": "string (Required)",
              "dosage": "string (Required)",
              "notes": "string (Optional)"
          }
      ],
      "procedures": [
          {
              "name": "string (Required)",
              "description": "string (Optional)"
          }
      ]
  }
  ```
- **Validations:**
  - For each prescription: `medicationName` and `dosage` are required.
  - For each procedure: `name` is required.

---

## 4. Patients API

### `GET /api/v1/Patients`
- **Description:** Retrieves a list of all patients in the system with their basic details and `medicalRecordId`.
- **Authorization:** `Admin`, `Dentist`, `Receptionist`
- **Request Body:** None
- **Response Structure:** Returns a list of `PatientListItemDto` objects.
