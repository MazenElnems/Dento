import { Injectable } from '@angular/core';

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  patientPhone: string;
  serviceId: string;
  serviceName: string;
  price: number;
  dateStr: string;
  time: string;
  dentistId: string;
  dentistName: string;
  status: 'upcoming' | 'completed' | 'cancelled';
  isEmergency?: boolean;
  paymentMethod?: 'Card' | 'Cash';
  paymentStatus?: 'Paid' | 'Pending';
}

export interface DentalRecord {
  id: string;
  patientId: string;
  patientName: string;
  dentistId: string;
  dentistName: string;
  date: string;
  diagnosis: string;
  procedurePerformed: string;
  clinicalNotes: string;
  treatmentPlan: string;
  files: string[];
}

export interface Prescription {
  id: string;
  patientId: string;
  patientName: string;
  dentistId: string;
  dentistName: string;
  date: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  duration: string;
  notes: string;
}

export interface DentistVacation {
  id: string;
  dentistEmail: string;
  dentistName: string;
  startDate: string;
  endDate: string;
}

export interface LeaveRequest {
  id: string;
  dentistEmail: string;
  dentistName: string;
  startDate: string;
  endDate: string;
  status: 'pending' | 'approved' | 'rejected';
  reason: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  constructor() {
    this.initDatabase();
  }

  private initDatabase() {
    if (!localStorage.getItem('dc_medical_histories')) {
      localStorage.setItem('dc_medical_histories', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_appointments')) {
      // Seed some appointments for admin analytics
      const seedApps = [
        { id: 'DC-8001', patientId: 'pat_01', patientName: 'أكرم شوايل', patientPhone: '01002930219', serviceId: 'cleaning', serviceName: 'Teeth Cleaning', price: 90, dateStr: 'Jul 08', time: '09:00 AM', dentistId: 'dentist_01', dentistName: 'د. مازن النمس', status: 'completed' },
        { id: 'DC-8002', patientId: 'pat_02', patientName: 'أحمد محمود', patientPhone: '01129384756', serviceId: 'implants', serviceName: 'Dental Implants', price: 1200, dateStr: 'Jul 09', time: '10:30 AM', dentistId: 'admin_01', dentistName: 'د. هنا بشرى', status: 'completed' },
        { id: 'DC-8003', patientId: 'pat_03', patientName: 'ليلى كريم', patientPhone: '01293847561', serviceId: 'whitening', serviceName: 'Teeth Whitening', price: 150, dateStr: 'Jul 10', time: '01:00 PM', dentistId: 'dentist_01', dentistName: 'د. مازن النمس', status: 'upcoming' },
        { id: 'DC-8004', patientId: 'pat_04', patientName: 'ياسر خالد', patientPhone: '01029384756', serviceId: 'ortho', serviceName: 'Orthodontics', price: 2500, dateStr: 'Jul 06', time: '11:15 AM', dentistId: 'admin_01', dentistName: 'د. هنا بشرى', status: 'cancelled' }
      ];

      localStorage.setItem('dc_appointments', JSON.stringify(seedApps));
    }
    if (!localStorage.getItem('dc_dental_records')) {
      localStorage.setItem('dc_dental_records', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_prescriptions')) {
      localStorage.setItem('dc_prescriptions', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_dentist_schedules')) {
      const seedSchedules = [
        { email: 'dentist@clinic.com', name: 'د. مازن النمس', workingDays: [0, 1, 2, 3, 4], hours: '09:00 - 17:00', slotLength: 30 },
        { email: 'admin@clinic.com', name: 'د. هنا بشرى', workingDays: [1, 2, 3, 4, 6], hours: '14:00 - 22:00', slotLength: 45 }
      ];
      localStorage.setItem('dc_dentist_schedules', JSON.stringify(seedSchedules));
    }
    if (!localStorage.getItem('dc_vacations')) {
      const seedVacations = [
        { id: 'VAC-01', dentistEmail: 'dentist@clinic.com', dentistName: 'د. مازن النمس', startDate: '2026-08-01', endDate: '2026-08-10' }
      ];
      localStorage.setItem('dc_vacations', JSON.stringify(seedVacations));
    }
    if (!localStorage.getItem('dc_leave_requests')) {
      const seedLeaves = [
        { id: 'LR-01', dentistEmail: 'dentist@clinic.com', dentistName: 'د. مازن النمس', startDate: '2026-07-20', endDate: '2026-07-22', status: 'pending', reason: 'Medical Conference' }
      ];
      localStorage.setItem('dc_leave_requests', JSON.stringify(seedLeaves));
    }
  }

  getAppointments(): Appointment[] {
    return JSON.parse(localStorage.getItem('dc_appointments') || '[]');
  }

  saveAppointment(app: Appointment) {
    const list = this.getAppointments();
    list.push(app);
    localStorage.setItem('dc_appointments', JSON.stringify(list));
  }

  updateAppointmentStatus(id: string, status: Appointment['status']) {
    const list = this.getAppointments();
    const app = list.find(a => a.id === id);
    if (app) {
      app.status = status;
      localStorage.setItem('dc_appointments', JSON.stringify(list));
    }
  }

  confirmCashPayment(id: string) {
    const list = this.getAppointments();
    const app = list.find(a => a.id === id);
    if (app) {
      app.paymentStatus = 'Paid';
      localStorage.setItem('dc_appointments', JSON.stringify(list));
    }
  }

  getMedicalHistory(userId: string) {
    const list = JSON.parse(localStorage.getItem('dc_medical_histories') || '[]');
    return list.find((m: any) => m.userId === userId) || null;
  }

  getDentalRecords(patientId: string): DentalRecord[] {
    const list = JSON.parse(localStorage.getItem('dc_dental_records') || '[]');
    return list.filter((r: any) => r.patientId === patientId);
  }

  addDentalRecord(record: DentalRecord) {
    const list = JSON.parse(localStorage.getItem('dc_dental_records') || '[]');
    list.push(record);
    localStorage.setItem('dc_dental_records', JSON.stringify(list));
  }

  getPrescriptions(patientId: string): Prescription[] {
    const list = JSON.parse(localStorage.getItem('dc_prescriptions') || '[]');
    return list.filter((p: any) => p.patientId === patientId);
  }

  addPrescription(prescription: Prescription) {
    const list = JSON.parse(localStorage.getItem('dc_prescriptions') || '[]');
    list.push(prescription);
    localStorage.setItem('dc_prescriptions', JSON.stringify(list));
  }

  getDentistSchedules() {
    return JSON.parse(localStorage.getItem('dc_dentist_schedules') || '[]');
  }

  saveDentistSchedules(schedules: any[]) {
    localStorage.setItem('dc_dentist_schedules', JSON.stringify(schedules));
  }

  // Vacations
  getVacations(): DentistVacation[] {
    return JSON.parse(localStorage.getItem('dc_vacations') || '[]');
  }

  saveVacation(vacation: DentistVacation) {
    const list = this.getVacations();
    list.push(vacation);
    localStorage.setItem('dc_vacations', JSON.stringify(list));
  }

  deleteVacation(id: string) {
    let list = this.getVacations();
    list = list.filter(v => v.id !== id);
    localStorage.setItem('dc_vacations', JSON.stringify(list));
  }

  // Leaves
  getLeaveRequests(): LeaveRequest[] {
    return JSON.parse(localStorage.getItem('dc_leave_requests') || '[]');
  }

  saveLeaveRequest(request: LeaveRequest) {
    const list = this.getLeaveRequests();
    list.push(request);
    localStorage.setItem('dc_leave_requests', JSON.stringify(list));
  }

  updateLeaveStatus(id: string, status: LeaveRequest['status']) {
    const list = this.getLeaveRequests();
    const req = list.find(r => r.id === id);
    if (req) {
      req.status = status;
      localStorage.setItem('dc_leave_requests', JSON.stringify(list));

      // If approved, automatically add to vacations list
      if (status === 'approved') {
        this.saveVacation({
          id: 'VAC-' + Math.floor(100 + Math.random() * 900),
          dentistEmail: req.dentistEmail,
          dentistName: req.dentistName,
          startDate: req.startDate,
          endDate: req.endDate
        });
      }
    }
  }

  updateUserStatus(userId: string, status: 'active' | 'inactive') {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    const user = users.find((u: any) => u.id === userId);
    if (user) {
      user.status = status;
      localStorage.setItem('dc_users', JSON.stringify(users));
      this.updateSharedCookie(users);
    }
  }

  addUser(user: any) {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    users.push(user);
    localStorage.setItem('dc_users', JSON.stringify(users));
    this.updateSharedCookie(users);
  }

  updateUser(user: any) {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    const index = users.findIndex((u: any) => u.id === user.id);
    if (index >= 0) {
      users[index] = user;
      localStorage.setItem('dc_users', JSON.stringify(users));
      this.updateSharedCookie(users);
    }
  }

  private updateSharedCookie(users: any[]) {
    try {
      const activeDentists = users.filter((u: any) => (u.role === 'dentist' || u.role === 'admin') && u.status === 'active');
      const optimized = activeDentists.map(d => ({
        id: d.id,
        name: d.name,
        specialty: d.specialty,
        imageUrl: d.imageUrl
      }));
      document.cookie = "dc_shared_users=" + encodeURIComponent(JSON.stringify(optimized)) + "; path=/; max-age=31536000";
    } catch (e) {
      console.error('Error writing optimized shared cookie:', e);
    }
  }
}
