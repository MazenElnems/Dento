import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PatientDashboard } from './patient/patient-dashboard';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, PatientDashboard],
  template: `<app-patient-dashboard></app-patient-dashboard>`,
  styles: [`
    :host {
      display: block;
      min-height: calc(100vh - 96px);
      background: #f8fafc;
    }
  `]
})
export class Dashboard {
  authService = inject(AuthService);
  router = inject(Router);

  constructor() {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/login']);
    } else if (user.role !== 'patient') {
      alert('This dashboard is only for patients. Staff members should use the admin portal.');
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }
}
