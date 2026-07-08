import { Injectable, signal } from '@angular/core';

export interface MedicalHistory {
  userId: string;
  bloodPressure: string;
  diabetes: boolean;
  heartDisease: boolean;
  allergies: string;
  currentMedications: string;
  previousSurgeries: string;
  smokingStatus: string;
  additionalNotes: string;
  updatedAt: string;
}

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  patientPhone: string;
  serviceId: string;
  serviceName: string;
  price: number;
  dateStr: string; // e.g. "Jul 10"
  time: string; // e.g. "09:00 AM"
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
  files: string[]; // Base64 data or Mock URLs
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

export interface SystemNotification {
  id: string;
  userId: string; // For patient/dentist specific notification
  title: string;
  message: string;
  date: string;
  read: boolean;
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
      // Seed some past and upcoming appointments for demo
      const seedAppointments: Appointment[] = [
        {
          id: 'DC-921827',
          patientId: 'user_patient',
          patientName: 'أحمد علي',
          patientPhone: '01012345678',
          serviceId: 'cleaning',
          serviceName: 'Teeth Cleaning & Hygiene',
          price: 90,
          dateStr: 'Jul 05',
          time: '10:30 AM',
          dentistId: 'user_dentist',
          dentistName: 'د. مازن النمس',
          status: 'completed'
        },
        {
          id: 'DC-883719',
          patientId: 'user_patient',
          patientName: 'أحمد علي',
          patientPhone: '01012345678',
          serviceId: 'whitening',
          serviceName: 'Teeth Whitening',
          price: 150,
          dateStr: 'Jul 12',
          time: '01:00 PM',
          dentistId: 'user_dentist',
          dentistName: 'د. مازن النمس',
          status: 'upcoming'
        }
      ];
      localStorage.setItem('dc_appointments', JSON.stringify(seedAppointments));
    }
    if (!localStorage.getItem('dc_dental_records')) {
      const seedRecords: DentalRecord[] = [
        {
          id: 'REC-101',
          patientId: 'user_patient',
          patientName: 'أحمد علي',
          dentistId: 'user_dentist',
          dentistName: 'د. مازن النمس',
          date: '2026-07-05',
          diagnosis: 'Severe plaque accumulation and minor gingival inflammation.',
          procedurePerformed: 'Scaling, root planing, and professional teeth polishing.',
          clinicalNotes: 'Plaque removed successfully. Patient instructed to brush twice daily and use dental floss.',
          treatmentPlan: 'Follow up in 6 months for regular cleaning.',
          files: ['assets/images/gallery-after-1.jpg']
        }
      ];
      localStorage.setItem('dc_dental_records', JSON.stringify(seedRecords));
    }
    if (!localStorage.getItem('dc_prescriptions')) {
      const seedPrescriptions: Prescription[] = [
        {
          id: 'RX-501',
          patientId: 'user_patient',
          patientName: 'أحمد علي',
          dentistId: 'user_dentist',
          dentistName: 'د. مازن النمس',
          date: '2026-07-05',
          medicationName: 'Chlorhexidine Gluconate 0.12% Mouthwash',
          dosage: '15 ml',
          frequency: 'Twice daily',
          duration: '7 days',
          notes: 'Rinse for 30 seconds after brushing, do not swallow.'
        }
      ];
      localStorage.setItem('dc_prescriptions', JSON.stringify(seedPrescriptions));
    }
    if (!localStorage.getItem('dc_notifications')) {
      const seedNotifications: SystemNotification[] = [
        {
          id: 'NT-01',
          userId: 'all',
          title: 'Welcome to Dento!',
          message: 'Thank you for registering at our clinic. Please complete your medical history form to start booking.',
          date: new Date().toLocaleString(),
          read: false
        }
      ];
      localStorage.setItem('dc_notifications', JSON.stringify(seedNotifications));
    }
    if (!localStorage.getItem('dc_dentist_schedules')) {
      // Seed schedules for our main dentists
      const seedSchedules = [
        { email: 'dentist@clinic.com', name: 'د. مازن النمس', workingDays: [0, 1, 2, 3, 4], hours: '09:00 AM - 05:00 PM' },
        { email: 'admin@clinic.com', name: 'د. هنا بشرى', workingDays: [1, 2, 3, 4, 6], hours: '02:00 PM - 10:00 PM' }
      ];
      localStorage.setItem('dc_dentist_schedules', JSON.stringify(seedSchedules));
    }
  }

  // --- MEDICAL HISTORY ---
  getMedicalHistory(userId: string): MedicalHistory | null {
    const list: MedicalHistory[] = JSON.parse(localStorage.getItem('dc_medical_histories') || '[]');
    return list.find(m => m.userId === userId) || null;
  }

  saveMedicalHistory(history: MedicalHistory) {
    const list: MedicalHistory[] = JSON.parse(localStorage.getItem('dc_medical_histories') || '[]');
    const index = list.findIndex(m => m.userId === history.userId);
    if (index >= 0) {
      list[index] = { ...history, updatedAt: new Date().toLocaleString() };
    } else {
      list.push({ ...history, updatedAt: new Date().toLocaleString() });
    }
    localStorage.setItem('dc_medical_histories', JSON.stringify(list));
  }

  // --- APPOINTMENTS ---
  getAppointments(): Appointment[] {
    return JSON.parse(localStorage.getItem('dc_appointments') || '[]');
  }

  saveAppointment(app: Appointment) {
    const list = this.getAppointments();
    list.push(app);
    localStorage.setItem('dc_appointments', JSON.stringify(list));

    // Send notification
    this.addNotification({
      id: 'NT-' + Math.floor(1000 + Math.random() * 9000),
      userId: app.patientId,
      title: 'تم تأكيد الحجز',
      message: `تم حجز موعدك لـ ${app.serviceName} في ${app.dateStr} في تمام الساعة ${app.time} مع ${app.dentistName}.`,
      date: new Date().toLocaleString(),
      read: false
    });
  }

  updateAppointmentStatus(id: string, status: Appointment['status']) {
    const list = this.getAppointments();
    const app = list.find(a => a.id === id);
    if (app) {
      app.status = status;
      localStorage.setItem('dc_appointments', JSON.stringify(list));

      // Notify
      this.addNotification({
        id: 'NT-' + Math.floor(1000 + Math.random() * 9000),
        userId: app.patientId,
        title: status === 'cancelled' ? 'تم إلغاء الحجز' : 'تم تحديث حالة الحجز',
        message: `تم ${status === 'cancelled' ? 'إلغاء' : 'تحديث حالة'} حجز موعدك لـ ${app.serviceName} بتاريخ ${app.dateStr}.`,
        date: new Date().toLocaleString(),
        read: false
      });
    }
  }

  rescheduleAppointment(id: string, dateStr: string, time: string) {
    const list = this.getAppointments();
    const app = list.find(a => a.id === id);
    if (app) {
      const oldDate = app.dateStr;
      const oldTime = app.time;
      app.dateStr = dateStr;
      app.time = time;
      localStorage.setItem('dc_appointments', JSON.stringify(list));

      this.addNotification({
        id: 'NT-' + Math.floor(1000 + Math.random() * 9000),
        userId: app.patientId,
        title: 'تعديل موعد الحجز',
        message: `تم تعديل موعدك من (${oldDate} - ${oldTime}) ليصبح (${dateStr} - ${time}).`,
        date: new Date().toLocaleString(),
        read: false
      });
    }
  }

  // --- DENTAL RECORDS ---
  getDentalRecords(patientId: string): DentalRecord[] {
    const list: DentalRecord[] = JSON.parse(localStorage.getItem('dc_dental_records') || '[]');
    return list.filter(r => r.patientId === patientId);
  }

  getAllDentalRecords(): DentalRecord[] {
    return JSON.parse(localStorage.getItem('dc_dental_records') || '[]');
  }

  addDentalRecord(record: DentalRecord) {
    const list: DentalRecord[] = JSON.parse(localStorage.getItem('dc_dental_records') || '[]');
    list.push(record);
    localStorage.setItem('dc_dental_records', JSON.stringify(list));

    this.addNotification({
      id: 'NT-' + Math.floor(1000 + Math.random() * 9000),
      userId: record.patientId,
      title: 'سجل طبي جديد',
      message: `قام ${record.dentistName} بإضافة تقرير طبي جديد لملفك الشخصي.`,
      date: new Date().toLocaleString(),
      read: false
    });
  }

  // --- PRESCRIPTIONS ---
  getPrescriptions(patientId: string): Prescription[] {
    const list: Prescription[] = JSON.parse(localStorage.getItem('dc_prescriptions') || '[]');
    return list.filter(p => p.patientId === patientId);
  }

  getAllPrescriptions(): Prescription[] {
    return JSON.parse(localStorage.getItem('dc_prescriptions') || '[]');
  }

  addPrescription(prescription: Prescription) {
    const list: Prescription[] = JSON.parse(localStorage.getItem('dc_prescriptions') || '[]');
    list.push(prescription);
    localStorage.setItem('dc_prescriptions', JSON.stringify(list));

    this.addNotification({
      id: 'NT-' + Math.floor(1000 + Math.random() * 9000),
      userId: prescription.patientId,
      title: 'وصفة طبية جديدة',
      message: `قام ${prescription.dentistName} بإصدار وصفة طبية جديدة لـ ${prescription.medicationName}.`,
      date: new Date().toLocaleString(),
      read: false
    });
  }

  // --- NOTIFICATIONS ---
  getNotifications(userId: string): SystemNotification[] {
    const list: SystemNotification[] = JSON.parse(localStorage.getItem('dc_notifications') || '[]');
    return list.filter(n => n.userId === userId || n.userId === 'all');
  }

  addNotification(notif: SystemNotification) {
    const list: SystemNotification[] = JSON.parse(localStorage.getItem('dc_notifications') || '[]');
    list.push(notif);
    localStorage.setItem('dc_notifications', JSON.stringify(list));
  }

  markAllAsRead(userId: string) {
    const list: SystemNotification[] = JSON.parse(localStorage.getItem('dc_notifications') || '[]');
    list.forEach(n => {
      if (n.userId === userId || n.userId === 'all') {
        n.read = true;
      }
    });
    localStorage.setItem('dc_notifications', JSON.stringify(list));
  }

  // --- DENTIST SCHEDULE MANAGEMENT ---
  getDentistSchedules() {
    return JSON.parse(localStorage.getItem('dc_dentist_schedules') || '[]');
  }

  saveDentistSchedules(schedules: any[]) {
    localStorage.setItem('dc_dentist_schedules', JSON.stringify(schedules));
  }

  // --- USER ADMINISTRATION ---
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
