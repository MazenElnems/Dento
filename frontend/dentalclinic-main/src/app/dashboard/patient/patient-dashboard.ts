import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { DataService, Appointment, DentalRecord, Prescription, SystemNotification } from '../../services/data.service';

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './patient-dashboard.html',
  styleUrl: './patient-dashboard.css'
})
export class PatientDashboard implements OnInit {
  authService = inject(AuthService);
  dataService = inject(DataService);
  router = inject(Router);

  appointments: Appointment[] = [];
  dentalRecords: DentalRecord[] = [];
  prescriptions: Prescription[] = [];
  notifications: SystemNotification[] = [];
  hasMedicalHistory = false;

  // Reschedule Modal State
  showRescheduleModal = false;
  reschedulingAppId = '';
  newDate = '';
  newTime = '09:00 AM';
  availableTimes = ['09:00 AM', '09:45 AM', '10:30 AM', '11:15 AM', '01:00 PM', '01:45 PM', '02:30 PM', '03:15 PM', '04:00 PM'];

  ngOnInit() {
    const user = this.authService.currentUser();
    if (!user) return;

    this.loadDashboardData(user.id);
  }

  loadDashboardData(userId: string) {
    this.appointments = this.dataService.getAppointments().filter(a => a.patientId === userId);
    this.dentalRecords = this.dataService.getDentalRecords(userId);
    this.prescriptions = this.dataService.getPrescriptions(userId);
    this.notifications = this.dataService.getNotifications(userId);
    this.hasMedicalHistory = this.dataService.getMedicalHistory(userId) !== null;
  }

  getUpcomingAppointments(): Appointment[] {
    return this.appointments.filter(a => a.status === 'upcoming');
  }

  getPastAppointments(): Appointment[] {
    return this.appointments.filter(a => a.status === 'completed' || a.status === 'cancelled');
  }

  getFormattedDate(dateStr: string): { month: string; day: string } {
    if (!dateStr) return { month: '', day: '' };
    
    if (dateStr.includes('-')) {
      const parts = dateStr.split('-');
      if (parts.length === 3) {
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const monthIndex = parseInt(parts[1], 10) - 1;
        const monthName = months[monthIndex] || 'Jul';
        const dayVal = parseInt(parts[2], 10).toString();
        return { month: monthName, day: dayVal };
      }
    }
    
    const parts = dateStr.split(' ');
    if (parts.length >= 2) {
      return { month: parts[0], day: parts[1] };
    }
    
    return { month: 'Date', day: dateStr };
  }

  cancelAppointment(appId: string) {
    const confirmCancel = confirm('Are you sure you want to cancel this appointment?');
    if (!confirmCancel) return;

    this.dataService.updateAppointmentStatus(appId, 'cancelled');
    
    const user = this.authService.currentUser();
    if (user) this.loadDashboardData(user.id);
    
    alert('Appointment cancelled successfully.');
  }

  openRescheduleModal(appId: string) {
    this.reschedulingAppId = appId;
    this.showRescheduleModal = true;
  }

  closeRescheduleModal() {
    this.showRescheduleModal = false;
    this.reschedulingAppId = '';
  }

  submitReschedule() {
    if (!this.newDate || !this.newTime) {
      alert('Please select both date and time.');
      return;
    }

    // Format new date to standard string (e.g. Jul 15)
    const dateObj = new Date(this.newDate);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;

    this.dataService.rescheduleAppointment(this.reschedulingAppId, formattedDate, this.newTime);
    
    const user = this.authService.currentUser();
    if (user) this.loadDashboardData(user.id);

    this.closeRescheduleModal();
    alert('Appointment rescheduled successfully.');
  }

  markAllNotificationsRead() {
    const user = this.authService.currentUser();
    if (!user) return;

    this.dataService.markAllAsRead(user.id);
    this.loadDashboardData(user.id);
  }
}
