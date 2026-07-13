import { Component, OnInit, ViewChild, ElementRef, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AuthService, User } from '../../services/auth.service';
import { DataService, Appointment } from '../../services/data.service';

// ==========================================================

// ==========================================================
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
  @Input() activeSubTab = 'dashboard';
  @Output() activeSubTabChange = new EventEmitter<string>();
  @ViewChild('scheduleSection') scheduleSection?: ElementRef<HTMLElement>;

  appointments: Appointment[] = [];
  dentists: User[] = [];

  // Filter States
  selectedDentistFilter = 'all';
  selectedStatusFilter = 'all';
  selectedDateFilter = '';

  quickSearch = '';

  // Manual Booking Modal
  showBookingModal = false;
  isEmergencyBooking = false;

  // Normal Booking Form
  patientName = '';
  patientPhone = '';
  bookingService = 'cleaning';
  bookingDentistId = '';
  bookingDate = '';
  bookingTime = '09:00 AM';

  // Emergency Booking Form
  emergPatientName = '';
  emergPatientPhone = '';
  emergService = 'cleaning';
  emergDentistId = '';
  emergDate = '';
  emergTime = '09:00 AM';

  services = [
    { id: 'cleaning', name: 'Teeth Cleaning & Hygiene', price: 90 },
    { id: 'whitening', name: 'Teeth Whitening', price: 150 },
    { id: 'implants', name: 'Dental Implants', price: 1200 },
    { id: 'ortho', name: 'Orthodontics (Braces)', price: 2500 },
    { id: 'canal', name: 'Root Canal Treatment', price: 350 }
  ];

  availableTimes = ['09:00 AM', '09:45 AM', '10:30 AM', '11:15 AM', '01:00 PM', '01:45 PM', '02:30 PM', '03:15 PM', '04:00 PM'];

  getDentists: any[] = [];
  noDentists: string = "";

  ngOnInit() {
    this.loadData();
    this.authService.getAllDentists().subscribe({
      next: (res: any) => {
        if (res && res.success && res.data) {
          const backendDentists = res.data.map((d: any) => ({
            id: d.id,
            name: d.fullName,
            email: d.email || '',
            role: 'dentist',
            specialty: d.specialty,
            imageUrl: d.imageUrl
          }));
          const localDentists = this.dentists.filter(ld => !backendDentists.some((bd: any) => bd.id === ld.id));
          this.dentists = [...backendDentists, ...localDentists];
          if (this.dentists.length > 0) {
            this.bookingDentistId = this.dentists[0].id;
            this.emergDentistId = this.dentists[0].id;
          }
        }
      },
      error: (err) => {
        console.error('Error fetching dentists:', err);
      }
    });
  }

  loadData() {
    this.appointments = this.dataService.getAppointments();
    this.dentists = this.authService.getUsers().filter(u => u.role === 'dentist' || u.role === 'admin');

    if (this.dentists.length > 0) {
      this.bookingDentistId = this.dentists[0].id;
      this.emergDentistId = this.dentists[0].id;
    }
  }

  get todayAppointmentsCount(): number {
    return this.appointments.filter(a => a.status !== 'cancelled').length;
  }

  get emergencyCount(): number {
    return this.appointments.filter(a => a.isEmergency && a.status !== 'cancelled').length;
  }

  get availableDentistsCount(): number {
    return this.dentists.length;
  }

  getFilteredAppointments(): Appointment[] {
    return this.appointments.filter(app => {
      if (this.selectedDentistFilter !== 'all' && app.dentistId !== this.selectedDentistFilter) {
        return false;
      }
      if (this.selectedStatusFilter !== 'all' && app.status !== this.selectedStatusFilter) {
        return false;
      }
      if (this.selectedDateFilter) {
        const dateObj = new Date(this.selectedDateFilter);
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;
        if (app.dateStr !== formattedDate) {
          return false;
        }
      }
      if (this.quickSearch.trim()) {
        const q = this.quickSearch.trim().toLowerCase();
        if (!app.patientName.toLowerCase().includes(q)) {
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

  submitNormalBooking() {
    if (!this.patientName.trim() || !this.patientPhone.trim() || !this.bookingDate) {
      alert('Please fill out patient details and select a date.');
      return;
    }

    const dentist = this.dentists.find(d => d.id === this.bookingDentistId);
    const serviceObj = this.services.find(s => s.id === this.bookingService);
    if (!dentist || !serviceObj) return;

    const dateObj = new Date(this.bookingDate);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;

    const newApp: Appointment = {
      id: 'DC-' + Math.floor(100000 + Math.random() * 900000),
      patientId: 'walk-in_' + Math.floor(1000 + Math.random() * 9000),
      patientName: this.patientName,
      patientPhone: this.patientPhone,
      serviceId: serviceObj.id,
      serviceName: serviceObj.name,
      price: serviceObj.price,
      dateStr: formattedDate,
      time: this.bookingTime,
      dentistId: dentist.id,
      dentistName: dentist.name,
      status: 'upcoming',
      isEmergency: false,
      paymentMethod: 'Cash',
      paymentStatus: 'Pending'
    };

    this.dataService.saveAppointment(newApp);
    this.loadData();
    
    // Reset fields
    this.patientName = '';
    this.patientPhone = '';
    this.bookingDate = '';
    this.bookingTime = '09:00 AM';

    alert('Appointment booked successfully!');
  }

  submitEmergencyBooking() {
    if (!this.emergPatientName.trim() || !this.emergPatientPhone.trim() || !this.emergDate) {
      alert('Please fill out patient details and select a date.');
      return;
    }

    const dentist = this.dentists.find(d => d.id === this.emergDentistId);
    const serviceObj = this.services.find(s => s.id === this.emergService);
    if (!dentist || !serviceObj) return;

    const dateObj = new Date(this.emergDate);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;

    const newApp: Appointment = {
      id: 'DC-' + Math.floor(100000 + Math.random() * 900000),
      patientId: 'emergency_' + Math.floor(1000 + Math.random() * 9000),
      patientName: this.emergPatientName,
      patientPhone: this.emergPatientPhone,
      serviceId: serviceObj.id,
      serviceName: `${serviceObj.name} (Emergency)`,
      price: serviceObj.price,
      dateStr: formattedDate,
      time: this.emergTime,
      dentistId: dentist.id,
      dentistName: dentist.name,
      status: 'upcoming',
      isEmergency: true,
      paymentMethod: 'Cash',
      paymentStatus: 'Pending'
    };

    this.dataService.saveAppointment(newApp);
    this.loadData();

    // Reset fields
    this.emergPatientName = '';
    this.emergPatientPhone = '';
    this.emergDate = '';
    this.emergTime = '09:00 AM';

    alert('Emergency appointment allocated successfully!');
  }

  switchSubTab(tab: string) {
    this.activeSubTab = tab;
    this.activeSubTabChange.emit(tab);
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
  viewDailySchedule() {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = (today.getMonth() + 1).toString().padStart(2, '0');
    const dd = today.getDate().toString().padStart(2, '0');
    this.selectedDateFilter = `${yyyy}-${mm}-${dd}`;
    this.selectedStatusFilter = 'all';
    this.selectedDentistFilter = 'all';

    this.switchSubTab('schedule');
  }

  confirmCashPayment(appId: string) {
    this.dataService.confirmCashPayment(appId);
    this.loadData();
  }
}
