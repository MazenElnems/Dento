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
      localStorage.setItem('dc_appointments', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_dental_records')) {
      localStorage.setItem('dc_dental_records', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_prescriptions')) {
      localStorage.setItem('dc_prescriptions', JSON.stringify([]));
    }
    if (!localStorage.getItem('dc_dentist_schedules')) {
      const seedSchedules = [
        { email: 'dentist@clinic.com', name: 'د. مازن النمس', workingDays: [0, 1, 2, 3, 4], hours: '09:00 AM - 05:00 PM' },
        { email: 'admin@clinic.com', name: 'د. هنا بشرى', workingDays: [1, 2, 3, 4, 6], hours: '02:00 PM - 10:00 PM' }
      ];
      localStorage.setItem('dc_dentist_schedules', JSON.stringify(seedSchedules));
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

  updateUserStatus(userId: string, status: 'active' | 'inactive') {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    const user = users.find((u: any) => u.id === userId);
    if (user) {
      user.status = status;
      localStorage.setItem('dc_users', JSON.stringify(users));
    }
  }

  addUser(user: any) {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    users.push(user);
    localStorage.setItem('dc_users', JSON.stringify(users));
  }

  updateUser(user: any) {
    const usersJson = localStorage.getItem('dc_users') || '[]';
    const users = JSON.parse(usersJson);
    const index = users.findIndex((u: any) => u.id === user.id);
    if (index >= 0) {
      users[index] = user;
      localStorage.setItem('dc_users', JSON.stringify(users));
    }
  }
}
