import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

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
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = 'https://dentalclinicapis.runasp.net';

  currentUser = signal<User | null>(this.getStoredUser());

  private getStoredUser(): User | null {
    const userJson = localStorage.getItem('dc_admin_active_user');
    if (!userJson) return null;
    try {
      const user: User = JSON.parse(userJson);
      if (user && user.name && user.name.includes('@')) {
        const parts = user.name.split('@')[0];
        const nameParts = parts.split(/[^a-zA-Z]/).filter(Boolean);
        if (nameParts.length > 0) {
          user.name = nameParts
            .slice(0, 2)
            .map((p: string) => p.charAt(0).toUpperCase() + p.slice(1))
            .join(' ');
          localStorage.setItem('dc_admin_active_user', JSON.stringify(user));
        }
      }
      return user;
    } catch (e) {
      return null;
    }
  }

  getAuthToken(): string | null {
    return localStorage.getItem('dc_admin_auth_token');
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.getAuthToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    });
  }

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

  private extractUserFromToken(token: string): User | null {
    const payload = this.decodeJwt(token);
    if (!payload) return null;

    const id =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      payload['nameid'] ||
      payload['sub'] ||
      'unknown';
    const email =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
      payload['email'] ||
      'unknown@clinic.com';

    const firstName =
      payload['given_name'] ||
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] ||
      '';
    const lastName =
      payload['family_name'] ||
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'] ||
      '';

    let name = 'Staff User';
    if (firstName || lastName) {
      name = `${firstName} ${lastName}`.trim();
    } else {
      const rawName =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        payload['name'] ||
        payload['unique_name'] ||
        '';
      if (rawName.includes('@')) {
        const parts = rawName.split('@')[0];
        const nameParts = parts.split(/[^a-zA-Z]/).filter(Boolean);
        if (nameParts.length > 0) {
          name = nameParts
            .slice(0, 2)
            .map((p: string) => p.charAt(0).toUpperCase() + p.slice(1))
            .join(' ');
        } else {
          name = parts;
        }
      } else if (rawName) {
        name = rawName;
      }
    }

    let rawRole =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      payload['role'] ||
      'receptionist';
    if (Array.isArray(rawRole)) {
      rawRole = rawRole[0];
    }
    const role = rawRole.toString().toLowerCase() as User['role'];

    return { id, name, email, role, status: 'active' };
  }

  login(email: string, password: string): Observable<ApiResponse> {
    return this.http
      .post<ApiResponse>(`${this.baseUrl}/api/v1/Account/login`, { email, password })
      .pipe(
        tap((response) => {
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
              localStorage.setItem('dc_admin_auth_token', token);
              const user = this.extractUserFromToken(token);
              if (user) {
                localStorage.setItem('dc_admin_active_user', JSON.stringify(user));
                this.currentUser.set(user);
              }
            }
          }
        }),
      );
  }

  registerDentist(details: {
    firstName: string;
    middleName: string;
    lastName: string;
    email: string;
    password: string;
    specialty: string;
  }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/api/v1/Account/register-dentist`, details, {
      headers: this.getAuthHeaders(),
    });
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
      { headers: this.getAuthHeaders() },
    );
  }

  logout(): void {
    localStorage.removeItem('dc_admin_auth_token');
    localStorage.removeItem('dc_admin_active_user');
    this.currentUser.set(null);
  }

  getUsers(): User[] {
    // Return all users cached in local storage for administrative CRUD simulation
    const usersJson = localStorage.getItem('dc_users');
    return usersJson ? JSON.parse(usersJson) : [];
  }

  getAllDentists(): Observable<any> {
    console.log(this.getAuthToken());
    return this.http.get(`${this.baseUrl}/api/v1/Dentists`, { headers: this.getAuthHeaders() });
  }
}
