import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of } from 'rxjs';

export interface User {
  id: string;
  name: string;
  email: string;
  role: 'patient' | 'dentist' | 'receptionist' | 'admin';
  phone?: string;
  status?: 'active' | 'inactive';
}

export interface ApiResponse<T = any> {
  success: boolean;
  errorCode: string | null;
  statusCode: number;
  message: string | null;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = 'https://dentalclinicapis.runasp.net';

  // Active user state tracking using Angular signals
  currentUser = signal<User | null>(this.getStoredUser());

  constructor() {}

  private getStoredUser(): User | null {
    const userJson = localStorage.getItem('dc_active_user');
    return userJson ? JSON.parse(userJson) : null;
  }

  getAuthToken(): string | null {
    return localStorage.getItem('dc_auth_token');
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.getAuthToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    });
  }

  // Decodes base64 payload of a JWT token
  private decodeJwt(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const decoded = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (e) {
      console.error('JWT decoding failed:', e);
      return null;
    }
  }

  // Parses role and user data from the JWT claims
  private extractUserFromToken(token: string): User | null {
    const payload = this.decodeJwt(token);
    if (!payload) return null;

    const id = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'] || 'unknown';
    const email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || 'unknown@clinic.com';
    const name = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['name'] || payload['unique_name'] || 'User';
    
    let rawRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || 'patient';
    if (Array.isArray(rawRole)) {
      rawRole = rawRole[0];
    }
    const role = rawRole.toString().toLowerCase() as User['role'];

    return { id, name, email, role, status: 'active' };
  }

  login(email: string, password: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/login`, { email, password }).pipe(
      tap(response => {
        if (response.success && response.data) {
          let token = '';
          if (typeof response.data === 'string') {
            token = response.data;
          } else if (response.data && response.data.token) {
            token = response.data.token;
          } else if (response.data && response.data.jwtToken) {
            token = response.data.jwtToken;
          }

          if (token) {
            localStorage.setItem('dc_auth_token', token);
            const user = this.extractUserFromToken(token);
            if (user) {
              localStorage.setItem('dc_active_user', JSON.stringify(user));
              this.currentUser.set(user);
            }
          }
        }
      })
    );
  }

  register(details: {
    firstName: string;
    middleName: string;
    lastName: string;
    phone: string;
    gender: 'Male' | 'Female';
    birthDate: string;
    email: string;
    password: string;
  }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/register`, details);
  }

  verifyEmail(code: string, userId: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/verify-email`, { code, userId });
  }

  sendVerificationCode(email: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/send-verification-code`, { email });
  }

  forgetPassword(email: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/forget-password`, { email });
  }

  resetPassword(details: {
    userId: string;
    token: string;
    newPassword: string;
  }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/reset-password`, details);
  }

  registerDentist(details: {
    firstName: string;
    middleName: string;
    lastName: string;
    email: string;
    password: string;
    specialty: string;
  }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(
      `${this.baseUrl}/api/v1/Account/register-dentist`,
      details,
      { headers: this.getAuthHeaders() }
    );
  }

  registerReceptionist(details: {
    firstName: string;
    middleName: string;
    lastName: string;
    phone: string;
    gender: 'Male' | 'Female';
    birthDate: string;
    email: string;
    password: string;
  }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(
      `${this.baseUrl}/api/v1/Account/register-receptionist`,
      details,
      { headers: this.getAuthHeaders() }
    );
  }

  logout(): void {
    localStorage.removeItem('dc_auth_token');
    localStorage.removeItem('dc_active_user');
    this.currentUser.set(null);
  }
}
