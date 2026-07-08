import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../services/auth.service';
import { DataService, Appointment } from '../../services/data.service';

@Component({
  selector: 'app-receptionist-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './receptionist-dashboard.html',
  styleUrl: './receptionist-dashboard.css'
})
export class ReceptionistDashboard implements OnInit {
  authService = inject(AuthService);
  dataService = inject(DataService);

  appointments: Appointment[] = [];
  dentists: User[] = [];

  // Filter States
  selectedDentistFilter = 'all';
  selectedStatusFilter = 'all';
  selectedDateFilter = '';

  // Manual Booking Modal
  showBookingModal = false;
  isEmergencyBooking = false;

  // Form Fields
  patientName = '';
  patientPhone = '';
  bookingService = 'cleaning';
  bookingDentistId = '';
  bookingDate = '';
  bookingTime = '09:00 AM';

  services = [
    { id: 'cleaning', name: 'Teeth Cleaning & Hygiene', price: 90 },
    { id: 'whitening', name: 'Teeth Whitening', price: 150 },
    { id: 'implants', name: 'Dental Implants', price: 1200 },
    { id: 'ortho', name: 'Orthodontics (Braces)', price: 2500 },
    { id: 'canal', name: 'Root Canal Treatment', price: 350 }
  ];

  availableTimes = ['09:00 AM', '09:45 AM', '10:30 AM', '11:15 AM', '01:00 PM', '01:45 PM', '02:30 PM', '03:15 PM', '04:00 PM'];

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.appointments = this.dataService.getAppointments();
    this.dentists = this.authService.getUsers().filter(u => u.role === 'dentist' || u.role === 'admin');
    
    // Set default dentist for booking
    if (this.dentists.length > 0) {
      this.bookingDentistId = this.dentists[0].id;
    }
  }

  getFilteredAppointments(): Appointment[] {
    return this.appointments.filter(app => {
      // Dentist filter
      if (this.selectedDentistFilter !== 'all' && app.dentistId !== this.selectedDentistFilter) {
        return false;
      }
      // Status filter
      if (this.selectedStatusFilter !== 'all' && app.status !== this.selectedStatusFilter) {
        return false;
      }
      // Date filter
      if (this.selectedDateFilter) {
        const dateObj = new Date(this.selectedDateFilter);
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;
        if (app.dateStr !== formattedDate) {
          return false;
        }
      }
      return true;
    });
  }

  openBookingModal(isEmergency: boolean) {
    this.isEmergencyBooking = isEmergency;
    this.showBookingModal = true;
  }

  closeBookingModal() {
    this.showBookingModal = false;
    this.patientName = '';
    this.patientPhone = '';
    this.bookingDate = '';
    this.bookingTime = '09:00 AM';
  }

  submitBooking() {
    if (!this.patientName.trim() || !this.patientPhone.trim() || !this.bookingDate) {
      alert('Please fill out patient details and select a date.');
      return;
    }

    const dentist = this.dentists.find(d => d.id === this.bookingDentistId);
    const serviceObj = this.services.find(s => s.id === this.bookingService);
    if (!dentist || !serviceObj) return;

    // Format new date to standard string (e.g. Jul 15)
    const dateObj = new Date(this.bookingDate);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;

    const newApp: Appointment = {
      id: 'DC-' + Math.floor(100000 + Math.random() * 900000),
      patientId: 'walk-in_' + Math.floor(1000 + Math.random() * 9000), // Simulate unregistered patient ID
      patientName: this.patientName,
      patientPhone: this.patientPhone,
      serviceId: serviceObj.id,
      serviceName: this.isEmergencyBooking ? `${serviceObj.name} (Emergency)` : serviceObj.name,
      price: serviceObj.price,
      dateStr: formattedDate,
      time: this.bookingTime,
      dentistId: dentist.id,
      dentistName: dentist.name,
      status: 'upcoming',
      isEmergency: this.isEmergencyBooking
    };

    this.dataService.saveAppointment(newApp);
    this.loadData();
    this.closeBookingModal();
    alert(this.isEmergencyBooking ? 'Emergency slot booked successfully!' : 'Appointment booked successfully!');
  }

  cancelAppointment(appId: string) {
    const confirmCancel = confirm('Are you sure you want to cancel this appointment?');
    if (!confirmCancel) return;

    this.dataService.updateAppointmentStatus(appId, 'cancelled');
    this.loadData();
  }

  completeAppointment(appId: string) {
    this.dataService.updateAppointmentStatus(appId, 'completed');
    this.loadData();
  }
}
