# Software Requirements Specification (SRS)
## Dento — Dental Clinic Management System
---

## 1. Introduction

### 1.1 Purpose
This document specifies the software requirements for **Dento**, a full-stack dental clinic management system. It covers the Angular front-end client, the ASP.NET Core backend API, and the SQL Server database, describing functional and non-functional requirements, system architecture, data design, and interfaces between components.

### 1.2 Scope
Dento is a complete system supporting the core operations of a dental clinic:
- Multi-role user management (Admin, Dentist, Receptionist, Patient)
- Appointment scheduling and lifecycle management
- Payment processing for appointments, with support for retries, idempotency, and financial auditability
- A versioned public API consumed by the Angular client
- Role-based web interfaces for each user type
- Auditing of sensitive data changes across the system

The system consists of three layers: an Angular single-page application (client), an ASP.NET Core Web API (backend), and a SQL Server database.

### 1.3 Definitions, Acronyms, and Abbreviations
| Term | Definition |
|---|---|
| SRS | Software Requirements Specification |
| SPA | Single-Page Application |
| TPT | Table-Per-Type (EF Core inheritance mapping strategy) |
| EF Core | Entity Framework Core, the ORM used by the backend |
| Idempotency Key | A unique client-supplied key used to prevent duplicate processing of a request |
| Ledger | An append-only record of financial transactions/events |
| PaymentIntent | A Paymob/Stripe-style object representing an intended payment before confirmation |
| JWT | JSON Web Token, used for API authentication |

### 1.4 References
- Paymob API documentation
- Stripe payment integration model (used as an architectural reference)
- ASP.NET Core Identity documentation
- Entity Framework Core documentation
- Angular documentation

---

## 2. Overall Description

### 2.1 Product Perspective
Dento is a three-tier system:
- **Client tier:** An Angular SPA providing role-specific interfaces for Admins, Dentists, Receptionists, and Patients.
- **Application tier:** An ASP.NET Core Web API implementing business logic, authentication, and payment processing, built with Clean Architecture, Repository, and Unit of Work patterns.
- **Data tier:** A SQL Server database accessed via EF Core.

The Angular client communicates with the backend exclusively through the versioned REST API over HTTPS.

### 2.2 User Classes and Characteristics
| Role | Description |
|---|---|
| **Admin** | Manages clinic-wide settings, staff accounts, and has full visibility into appointments and payments. |
| **Dentist** | Manages their own schedule and appointments; views patient and treatment history. |
| **Receptionist** | Books and manages appointments on behalf of patients; handles day-to-day scheduling. |
| **Patient** | Books appointments, views appointment history, and makes payments. |

All four roles are implemented via ASP.NET Core Identity on the backend, with role-specific profile data modeled as separate entities rather than through EF Core inheritance (see Section 7.2). The Angular client renders a distinct dashboard and route set for each role, guarded by route guards tied to the user's JWT claims.

### 2.3 Operating Environment
- **Frontend:** Angular SPA (TypeScript), served as static assets
- **Backend Framework:** ASP.NET Core (Web API)
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Payment Gateway:** Paymob
- **Hosting:** Cloud-hosted (containerized deployment assumed), with the Angular build served separately or via a reverse proxy in front of the API

### 2.4 Design and Implementation Constraints
- Financial data must be stored in an append-only manner; no in-place mutation of historical payment records.
- All payment-affecting endpoints must support idempotency to prevent duplicate charges from retries or double-clicks, including duplicate submissions from the Angular client (e.g., double-clicking "Pay").
- The public API must support versioning (V1/V2 coexistence) without breaking existing Angular client builds.
- Role-based data must avoid EF Core TPT inheritance in favor of Identity + 1:1 profile entities, for schema simplicity and query performance.
- The Angular client must never store refresh tokens in browser storage accessible to JavaScript; refresh tokens are managed via httpOnly cookies set by the backend.

### 2.5 Assumptions and Dependencies
- Paymob is available and reachable as the payment processor; the system must gracefully handle its outages or timeouts, and the Angular client must present clear feedback to the user during those states.
- The Angular client and ASP.NET Core API are deployed such that cookies (for refresh tokens) can be shared appropriately (e.g., same site or configured CORS/cookie policy).

---

## 3. System Architecture Overview

```
[ Angular SPA ]  <--HTTPS/REST-->  [ ASP.NET Core Web API ]  <--EF Core-->  [ SQL Server ]
                                            |
                                            v
                                     [ Paymob Gateway ]
```

- The Angular client consumes the versioned REST API for all data operations.
- The backend applies role-based authorization derived from the authenticated user's JWT.
- The backend is the sole component with direct access to the SQL Server database and to Paymob.

---

## 4. Frontend Requirements (Angular Client)

### 4.1 General
- **FR-F1.1:** The client shall be built as an Angular SPA consuming the Dento REST API.
- **FR-F1.2:** The client shall provide role-specific routing and dashboards for Admin, Dentist, Receptionist, and Patient roles, using route guards based on the authenticated user's role claim.
- **FR-F1.3:** The client shall handle access token attachment to outgoing API requests (e.g., via an HTTP interceptor) and silent refresh using the httpOnly refresh token cookie.
- **FR-F1.4:** The client shall redirect unauthenticated users to a login view when a request fails due to an expired/invalid session.

### 4.2 Authentication Views
- **FR-F2.1:** The client shall provide login and (where applicable) registration views for Patients.
- **FR-F2.2:** The client shall surface clear error states for failed login attempts, expired sessions, and revoked sessions (e.g., due to detected token reuse).

### 4.3 Appointment Management Views
- **FR-F3.1:** Patients and Receptionists shall be able to browse Dentist availability and book appointments through the client.
- **FR-F3.2:** Dentists shall have a schedule view showing their own upcoming and past appointments.
- **FR-F3.3:** Admins shall have a clinic-wide appointment view.
- **FR-F3.4:** The client shall reflect appointment payment status (derived from the backend's computed financial truth, not merely the `currentPaymentId` pointer).

### 4.4 Payment Views
- **FR-F4.1:** The client shall initiate payments through the backend-provided PaymentIntent flow, redirecting to or embedding Paymob's client-side payment UI as required by Paymob's integration model.
- **FR-F4.2:** The client shall generate and attach an idempotency key for each distinct payment attempt, and shall reuse the same key when automatically retrying a failed request due to network issues (not user-initiated retries).
- **FR-F4.3:** The client shall disable/guard the "Pay" action while a payment request is in flight, to reduce accidental duplicate submissions.
- **FR-F4.4:** The client shall poll or listen for payment confirmation status and update the UI once the backend confirms success via webhook-driven state.

### 4.5 Admin & Staff Management Views
- **FR-F5.1:** Admins shall be able to create, update, and deactivate staff accounts (Dentist, Receptionist) through the client.

---

## 5. Backend Requirements (ASP.NET Core API)

### 5.1 Authentication & Authorization
- **FR-B1.1:** The backend shall authenticate users via a two-token architecture (access token + refresh token).
- **FR-B1.2:** Refresh tokens shall be issued in httpOnly cookies to mitigate XSS-based token theft against the Angular client.
- **FR-B1.3:** The backend shall support refresh token rotation, issuing a new refresh token on each use and invalidating the previous one.
- **FR-B1.4:** The backend shall detect refresh token reuse (a previously-rotated-out token being reused) as a signal of token theft, and shall revoke the entire token family upon detection.
- **FR-B1.5:** The backend shall support server-side revocation of refresh tokens (e.g., on logout or suspected compromise).
- **FR-B1.6:** The backend shall enforce role-based access control for Admin, Dentist, Receptionist, and Patient roles on all relevant endpoints.

### 5.2 User & Profile Management
- **FR-B2.1:** The backend shall manage core identity (login, roles, credentials) via ASP.NET Core Identity.
- **FR-B2.2:** The backend shall maintain role-specific profile data (e.g., `DentistProfile`, `PatientProfile`) as separate entities linked to the Identity user via a 1:1 foreign key relationship.
- **FR-B2.3:** Admins shall be able to create, update, and deactivate staff accounts via dedicated endpoints.

### 5.3 Appointment Management
- **FR-B3.1:** The backend shall expose endpoints for booking, updating, and cancelling appointments, enforcing role-based rules on who may perform each action.
- **FR-B3.2:** Each Appointment shall maintain a `currentPaymentId` field acting as a navigation pointer to the most relevant Payment record — informational only, never the source of financial truth.
- **FR-B3.3:** The backend shall expose endpoints for Dentists to retrieve their own schedules, and for Admins to retrieve clinic-wide schedules.

### 5.4 Payment Processing
- **FR-B4.1:** The backend shall integrate with Paymob to process appointment payments.
- **FR-B4.2:** Every payment attempt (including retries) shall create a new, immutable `Payment` row rather than mutating an existing one.
- **FR-B4.3:** The backend shall determine the true financial status of an appointment by querying all associated `Payment` rows with a `succeeded` status, not by reading a single cached value.
- **FR-B4.4:** The backend shall support idempotency keys on payment-initiating endpoints, backed by a SQL Server `idempotency_keys` table, to prevent duplicate charge creation from client retries or double-clicks.
- **FR-B4.5:** The backend shall distinguish between network-level failures (e.g., timeout, no response) and HTTP-level failures (e.g., 4xx/5xx from Paymob) when handling `CreatePaymentIntent` calls, and shall handle each case appropriately (e.g., safe retry vs. surfacing an error to the client).
- **FR-B4.6:** The backend shall process Paymob webhook events to confirm payment completion, with protection against duplicate/replayed webhook events.
- **FR-B4.7:** The backend shall not introduce a separate `PaymentAttempt` entity distinct from `Payment` — retries are represented as additional `Payment` rows to keep the schema simple.

### 5.5 API Versioning
- **FR-B5.1:** The backend shall expose a versioned REST API (e.g., V1, V2) allowing multiple versions to coexist, so the Angular client can migrate between versions without a hard cutover.
- **FR-B5.2:** Swagger/OpenAPI documentation shall correctly reflect all active API versions and their endpoints.
- **FR-B5.3:** Service registration order (e.g., `AddApiVersioning` before `AddSwaggerGen`) and `DocInclusionPredicate` configuration shall ensure each versioned Swagger document only includes endpoints belonging to that version group.

### 5.6 Auditing
- **FR-B6.1:** The backend shall maintain a generic `AuditLog` table capturing field-level changes to auditable entities.
- **FR-B6.2:** Entities requiring audit tracking shall implement an `IAuditable` interface.
- **FR-B6.3:** Audit records shall be automatically generated via an override of `SaveChangesAsync`, capturing what changed, when, and by whom.

---

## 6. External Interface Requirements

### 6.1 Client–Backend Interface
- The Angular client shall communicate with the backend exclusively over HTTPS via the versioned REST API.
- Authentication state shall be conveyed via a bearer access token (attached per-request) and an httpOnly refresh token cookie (not accessible to Angular code).

### 6.2 Payment Gateway Interface (Paymob)
- The backend shall create PaymentIntents via Paymob's API, following a pattern analogous to Stripe's server-creates-intent → client-confirms → webhook-confirms flow.
- The backend shall expose a webhook endpoint to receive asynchronous payment confirmation events from Paymob.
- The backend shall validate/verify incoming webhook payloads before treating them as authoritative.
- The Angular client shall interact with Paymob only through the redirect/embedded flow initiated by the backend-issued PaymentIntent — never directly holding payment credentials.

---

## 7. Non-Functional Requirements

### 7.1 Security
- **NFR-1.1:** Refresh tokens shall never be accessible to client-side JavaScript (httpOnly cookies only).
- **NFR-1.2:** All payment and identity endpoints shall require authentication; role checks shall be enforced server-side regardless of what the Angular client's route guards permit.
- **NFR-1.3:** Token theft (via reuse detection) shall result in immediate revocation of the affected token family, forcing re-authentication on the client.

### 7.2 Data Integrity & Auditability
- **NFR-2.1:** Financial records shall be append-only; no destructive updates to historical `Payment` rows.
- **NFR-2.2:** All changes to auditable entities shall be traceable via the `AuditLog` table.

### 7.3 Reliability
- **NFR-3.1:** Duplicate payment requests (from client retries, double-clicks, or webhook replays) shall not result in duplicate charges, enforced via idempotency keys and webhook reuse detection.
- **NFR-3.2:** The backend shall differentiate transient network failures from definitive HTTP failures when calling external payment APIs, to avoid incorrectly treating an ambiguous failure as a successful or failed payment.

### 7.4 Maintainability
- **NFR-4.1:** The backend shall follow Clean Architecture principles, with Repository and Unit of Work patterns separating domain logic from data access.
- **NFR-4.2:** Schema decisions shall favor simplicity over speculative flexibility (e.g., rejecting a `PaymentAttempt` entity in favor of reusing `Payment` rows).
- **NFR-4.3:** The Angular client shall be structured into feature modules aligned with user roles, to keep role-specific UI logic isolated and maintainable.

### 7.5 Compatibility
- **NFR-5.1:** New API versions shall be introduced without breaking existing Angular client builds still targeting a prior version.

### 7.6 Usability
- **NFR-6.1:** The Angular client shall present role-appropriate navigation, hiding actions the current user's role is not authorized to perform.

---

## 8. Data Design Overview

### 8.1 Payment Data Model (SQL Server)
- `Payment`: one row per payment attempt (including retries), immutable once written, with a status field (e.g., `pending`, `succeeded`, `failed`).
- `Appointment.currentPaymentId`: a pointer to the most recent/relevant `Payment` row for UI/reference convenience — **not** used to determine financial truth.
- Financial truth for an appointment is derived by querying all related `Payment` rows for any with `succeeded` status.
- `idempotency_keys` table: stores client-supplied idempotency keys with enough metadata to detect and short-circuit duplicate payment-initiating requests.

### 8.2 Identity & Role Data Model
- ASP.NET Core Identity manages core user accounts and roles (Admin, Dentist, Receptionist, Patient) in SQL Server.
- Role-specific data is modeled via separate profile entities (`DentistProfile`, `PatientProfile`, etc.), each linked 1:1 to the Identity user via foreign key — avoiding EF Core TPT inheritance.

### 8.3 Auditing Data Model
- `AuditLog`: generic table capturing entity type, record id, changed fields (old/new values), timestamp, and acting user.
- Entities requiring audit tracking implement `IAuditable`; changes are captured automatically on `SaveChangesAsync`.

---

## 9. Appendix

### 9.1 Open Items / Future Considerations
- Formal SLA definitions for Paymob integration timeouts and retry policies.
- Rate limiting and abuse-prevention specifics for public-facing endpoints.
- Detailed notification requirements (e.g., appointment reminders) if added to scope.
- Angular state management approach (e.g., services with RxJS vs. a dedicated store) — not yet finalized.
