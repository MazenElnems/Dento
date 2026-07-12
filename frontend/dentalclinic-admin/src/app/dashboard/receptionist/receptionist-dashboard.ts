import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, of, delay } from 'rxjs';

// ==========================================================

// ==========================================================
export interface User {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: 'admin' | 'dentist' | 'receptionist';
  specialty?: string;
  avatarInitials?: string;
  onDuty?: boolean;
}

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  patientPhone: string;
  serviceId: string;
  serviceName: string;
  price: number;
  dateStr: string;
  isoDate: string;
  time: string;
  dentistId: string;
  dentistName: string;
  status: 'upcoming' | 'completed' | 'cancelled';
  isEmergency: boolean;
}

// ==========================================================

// ==========================================================
function relativeDate(offsetDays: number): { dateStr: string; isoDate: string } {
  const d = new Date();
  d.setDate(d.getDate() + offsetDays);
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const dateStr = `${months[d.getMonth()]} ${d.getDate().toString().padStart(2, '0')}`;
  const isoDate = d.toISOString().slice(0, 10);
  return { dateStr, isoDate };
}

const MOCK_USERS: User[] = [
  { id: 'usr-001', name: 'Dr. Amir Hassan', email: 'amir.hassan@brightsmile-clinic.com', phone: '01012345678', role: 'dentist', specialty: 'General & Cosmetic Dentistry', avatarInitials: 'AH', onDuty: true },
  { id: 'usr-002', name: 'Dr. Nourhan El-Sayed', email: 'nourhan.elsayed@brightsmile-clinic.com', phone: '01098765432', role: 'dentist', specialty: 'Orthodontics', avatarInitials: 'NE', onDuty: true },
  { id: 'usr-003', name: 'Dr. Karim Mostafa', email: 'karim.mostafa@brightsmile-clinic.com', phone: '01123456789', role: 'dentist', specialty: 'Endodontics (Root Canal)', avatarInitials: 'KM', onDuty: false },
  { id: 'usr-004', name: 'Dr. Salma Adel', email: 'salma.adel@brightsmile-clinic.com', phone: '01234567890', role: 'dentist', specialty: 'Oral Surgery & Implants', avatarInitials: 'SA', onDuty: true },
  { id: 'usr-005', name: 'Mona Fathy', email: 'mona.fathy@brightsmile-clinic.com', phone: '01055566677', role: 'admin', avatarInitials: 'MF', onDuty: true },
  { id: 'usr-006', name: 'Yara Tamer', email: 'yara.tamer@brightsmile-clinic.com', phone: '01199988877', role: 'receptionist', avatarInitials: 'YT', onDuty: true }
];

const MOCK_APPOINTMENTS: Appointment[] = [
  { id: 'DC-482913', patientId: 'pat-1001', patientName: 'Mahmoud Reda', patientPhone: '01011122233', serviceId: 'cleaning', serviceName: 'Teeth Cleaning & Hygiene', price: 90, ...relativeDate(0), time: '09:00 AM', dentistId: 'usr-001', dentistName: 'Dr. Amir Hassan', status: 'completed', isEmergency: false },
  { id: 'DC-482914', patientId: 'pat-1002', patientName: 'Farida Younis', patientPhone: '01022233344', serviceId: 'whitening', serviceName: 'Teeth Whitening', price: 150, ...relativeDate(0), time: '09:45 AM', dentistId: 'usr-002', dentistName: 'Dr. Nourhan El-Sayed', status: 'completed', isEmergency: false },
  { id: 'DC-482915', patientId: 'pat-1003', patientName: 'Omar Sherif', patientPhone: '01033344455', serviceId: 'canal', serviceName: 'Root Canal Treatment (Emergency)', price: 350, ...relativeDate(0), time: '10:30 AM', dentistId: 'usr-003', dentistName: 'Dr. Karim Mostafa', status: 'upcoming', isEmergency: true },
  { id: 'DC-482916', patientId: 'pat-1004', patientName: 'Layla Ibrahim', patientPhone: '01044455566', serviceId: 'ortho', serviceName: 'Orthodontics (Braces)', price: 2500, ...relativeDate(0), time: '11:15 AM', dentistId: 'usr-002', dentistName: 'Dr. Nourhan El-Sayed', status: 'upcoming', isEmergency: false },
  { id: 'DC-482917', patientId: 'pat-1005', patientName: 'Hassan Gaber', patientPhone: '01055566688', serviceId: 'implants', serviceName: 'Dental Implants', price: 1200, ...relativeDate(0), time: '01:00 PM', dentistId: 'usr-004', dentistName: 'Dr. Salma Adel', status: 'upcoming', isEmergency: false },
  { id: 'DC-482918', patientId: 'pat-1006', patientName: 'Nadia Fouad', patientPhone: '01066677799', serviceId: 'cleaning', serviceName: 'Teeth Cleaning & Hygiene', price: 90, ...relativeDate(0), time: '01:45 PM', dentistId: 'usr-001', dentistName: 'Dr. Amir Hassan', status: 'cancelled', isEmergency: false },
  { id: 'DC-482919', patientId: 'pat-1007', patientName: 'Ziad Kamal', patientPhone: '01077788800', serviceId: 'canal', serviceName: 'Root Canal Treatment (Emergency)', price: 350, ...relativeDate(0), time: '02:30 PM', dentistId: 'usr-003', dentistName: 'Dr. Karim Mostafa', status: 'upcoming', isEmergency: true },
  { id: 'DC-482920', patientId: 'pat-1008', patientName: 'Rania Hosny', patientPhone: '01088899911', serviceId: 'whitening', serviceName: 'Teeth Whitening', price: 150, ...relativeDate(0), time: '03:15 PM', dentistId: 'usr-004', dentistName: 'Dr. Salma Adel', status: 'upcoming', isEmergency: false },
  { id: 'DC-482921', patientId: 'pat-1009', patientName: 'Tarek Aboul-Fotouh', patientPhone: '01099900022', serviceId: 'implants', serviceName: 'Dental Implants', price: 1200, ...relativeDate(1), time: '09:00 AM', dentistId: 'usr-004', dentistName: 'Dr. Salma Adel', status: 'upcoming', isEmergency: false },
  { id: 'DC-482922', patientId: 'pat-1010', patientName: 'Dina Magdy', patientPhone: '01010011122', serviceId: 'cleaning', serviceName: 'Teeth Cleaning & Hygiene', price: 90, ...relativeDate(1), time: '10:30 AM', dentistId: 'usr-001', dentistName: 'Dr. Amir Hassan', status: 'upcoming', isEmergency: false },
  { id: 'DC-482923', patientId: 'pat-1011', patientName: 'Sherif Anwar', patientPhone: '01021133445', serviceId: 'ortho', serviceName: 'Orthodontics (Braces)', price: 2500, ...relativeDate(1), time: '11:15 AM', dentistId: 'usr-002', dentistName: 'Dr. Nourhan El-Sayed', status: 'upcoming', isEmergency: false },
  { id: 'DC-482924', patientId: 'pat-1012', patientName: 'Aya Khalil', patientPhone: '01032244556', serviceId: 'whitening', serviceName: 'Teeth Whitening', price: 150, ...relativeDate(2), time: '01:00 PM', dentistId: 'usr-002', dentistName: 'Dr. Nourhan El-Sayed', status: 'upcoming', isEmergency: false },
  { id: 'DC-482925', patientId: 'pat-1013', patientName: 'Mostafa Salah', patientPhone: '01043355667', serviceId: 'canal', serviceName: 'Root Canal Treatment', price: 350, ...relativeDate(2), time: '02:30 PM', dentistId: 'usr-003', dentistName: 'Dr. Karim Mostafa', status: 'upcoming', isEmergency: false },
  { id: 'DC-482910', patientId: 'pat-1014', patientName: 'Heba Nabil', patientPhone: '01054466778', serviceId: 'cleaning', serviceName: 'Teeth Cleaning & Hygiene', price: 90, ...relativeDate(-1), time: '09:45 AM', dentistId: 'usr-001', dentistName: 'Dr. Amir Hassan', status: 'completed', isEmergency: false },
  { id: 'DC-482911', patientId: 'pat-1015', patientName: 'Waleed Fathy', patientPhone: '01065577889', serviceId: 'implants', serviceName: 'Dental Implants', price: 1200, ...relativeDate(-1), time: '11:15 AM', dentistId: 'usr-004', dentistName: 'Dr. Salma Adel', status: 'completed', isEmergency: false },
  { id: 'DC-482912', patientId: 'pat-1016', patientName: 'Mariam Sabry', patientPhone: '01076688990', serviceId: 'ortho', serviceName: 'Orthodontics (Braces)', price: 2500, ...relativeDate(-1), time: '01:45 PM', dentistId: 'usr-002', dentistName: 'Dr. Nourhan El-Sayed', status: 'cancelled', isEmergency: false }
];

// ==========================================================

// ==========================================================
class AuthService {
  getUsers(): User[] {
    return MOCK_USERS;
  }

  getAllDentists(): Observable<User[]> {
    const dentists = MOCK_USERS.filter(u => u.role === 'dentist');
    return of(dentists).pipe(delay(400));
  }
}

class DataService {
  private appointments: Appointment[] = MOCK_APPOINTMENTS.map(a => ({ ...a }));

  getAppointments(): Appointment[] {
    return this.appointments;
  }

  saveAppointment(appointment: Appointment): void {
    this.appointments = [...this.appointments, appointment];
  }

  updateAppointmentStatus(appId: string, status: Appointment['status']): void {
    this.appointments = this.appointments.map(a =>
      a.id === appId ? { ...a, status } : a
    );
  }
}

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
  authService = new AuthService();
  dataService = new DataService();
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

  getDentists: any[] = [];
  noDentists: string = "";

  ngOnInit() {
    this.loadData();
    this.authService.getAllDentists().subscribe({
      next: (res) => console.log(res),
      error: (err) => this.noDentists = "No Dentists"
    });
  }

  loadData() {
    this.appointments = this.dataService.getAppointments();
    this.dentists = this.authService.getUsers().filter(u => u.role === 'dentist' || u.role === 'admin');

    if (this.dentists.length > 0) {
      this.bookingDentistId = this.dentists[0].id;
    }
  }

  get completedCount(): number {
    return this.appointments.filter(a => a.status === 'completed').length;
  }

  get upcomingCount(): number {
    return this.appointments.filter(a => a.status === 'upcoming').length;
  }

  get emergencyCount(): number {
    return this.appointments.filter(a => a.isEmergency && a.status === 'upcoming').length;
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

  submitBooking() {
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
      serviceName: this.isEmergencyBooking ? `${serviceObj.name} (Emergency)` : serviceObj.name,
      price: serviceObj.price,
      dateStr: formattedDate,
      isoDate: this.bookingDate,
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

  viewDailySchedule() {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = (today.getMonth() + 1).toString().padStart(2, '0');
    const dd = today.getDate().toString().padStart(2, '0');
    this.selectedDateFilter = `${yyyy}-${mm}-${dd}`;
    this.selectedStatusFilter = 'all';
    this.selectedDentistFilter = 'all';

    this.scheduleSection?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
