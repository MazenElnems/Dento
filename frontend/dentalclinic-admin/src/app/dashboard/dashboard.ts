import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { DentistDashboard } from './dentist/dentist-dashboard';
import { ReceptionistDashboard } from './receptionist/receptionist-dashboard';
import { AdminDashboard } from './admin/admin-dashboard';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    DentistDashboard,
    ReceptionistDashboard,
    AdminDashboard
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  authService = inject(AuthService);
  router = inject(Router);

  // Active sub-tab state shared with roles (like admin)
  activeSubTab = 'dashboard';

  constructor() {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    if (user.role === 'patient') {
      alert('Unauthorized access. Redirecting...');
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
