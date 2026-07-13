import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService, ApiResponse } from './auth.service';

export interface Dentist {
  id: string;
  fullName: string;
  specialty: string;
  consultationFee: number;
  scheduleId: string;
  imageUrl: string;
  yearsOfExperience: number;
}

export interface Slot {
  id: string;
  from: string;
  to: string;
}

export interface DaySchedule {
  date: string;
  slots: Slot[];
}

export interface DentistSchedule {
  timeZone: any;
  schedule: DaySchedule[];
}

export interface BookedAppointmentResponse {
  id: string;
  status: string;
  slotId: string;
  slotStatus: string;
  lockedUntil: string;
  paymentDeadline: string;
}

export interface AppointmentDetails {
  id: string;
  status: 'Pending' | 'Confirmed' | 'Failed' | 'Canceled';
  appointmentType: string;
  createdAt: string;
  confirmedAt: string | null;
  canceledAt: string | null;
  slotId: string;
  slotDate: string;
  slotFrom: string;
  slotTo: string;
  slotStatus: string;
  slotLockedUntil: string;
  dentistId: string;
  dentistName: string;
  dentistSpecialty: string;
  consultationFee: number;
  paymentId: string | null;
  paymentStatus: string | null;
  paymentAmount: number | null;
  paymentCurrency: string | null;
}

export interface PaymentIntentResponse {
  appointmentId: string;
  clientSecret: string;
  publicKey: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = 'https://dentalclinicapis.runasp.net';

  constructor() {}

  // 1. Fetch available dentists list
  getDentists(): Observable<ApiResponse<Dentist[]>> {
    return this.http.get<ApiResponse<Dentist[]>>(
      `${this.baseUrl}/api/v1/Dentists`,
      { headers: this.authService.getAuthHeaders() }
    );
  }

  // 2. Fetch specific dentist slots schedule
  getSchedule(scheduleId: string): Observable<ApiResponse<DentistSchedule>> {
    return this.http.get<ApiResponse<DentistSchedule>>(
      `${this.baseUrl}/api/v1/Schedules/${scheduleId}`,
      { headers: this.authService.getAuthHeaders() }
    );
  }

  // 3. Book slot (Starts 10-minute lock)
  bookAppointment(slotId: string): Observable<ApiResponse<BookedAppointmentResponse>> {
    return this.http.post<ApiResponse<BookedAppointmentResponse>>(
      `${this.baseUrl}/api/v1/Appointments`,
      { slotId },
      { headers: this.authService.getAuthHeaders() }
    );
  }

  // 4. Retrieve appointment verification status details
  getAppointment(id: string): Observable<ApiResponse<AppointmentDetails>> {
    return this.http.get<ApiResponse<AppointmentDetails>>(
      `${this.baseUrl}/api/v1/Appointments/${id}`,
      { headers: this.authService.getAuthHeaders() }
    );
  }

  // 5. Initialize payment with Paymob or Cash
  createPayment(appointmentId: string, idempotencyKey: string, paymentType: 'Cash' | 'Online' = 'Online'): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.baseUrl}/api/v1/Payments/create-payment`,
      { appointmentId, idempotencyKey, paymentType },
      { headers: this.authService.getAuthHeaders() }
    );
  }
}
