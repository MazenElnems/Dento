import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../services/auth.service';
import { DataService, Appointment, DentalRecord, Prescription } from '../../services/data.service';

@Component({
  selector: 'app-dentist-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dentist-dashboard.html',
  styleUrl: './dentist-dashboard.css'
})
export class DentistDashboard implements OnInit {
  authService = inject(AuthService);
  dataService = inject(DataService);

  appointments: Appointment[] = [];
  patients: User[] = [];
  selectedPatient: User | null = null;
  selectedPatientHistory: any = null;
  selectedPatientRecords: DentalRecord[] = [];
  selectedPatientPrescriptions: Prescription[] = [];

  // Tab control
  activeTab: 'schedule' | 'patients' = 'schedule';

  // Forms states
  showRecordModal = false;
  showPrescriptionModal = false;
  showFollowUpModal = false;

  // New Record Form Fields
  diagnosis = '';
  procedurePerformed = '';
  clinicalNotes = '';
  treatmentPlan = '';
  uploadedFile = '';

  // New Prescription Form Fields
  medicationName = '';
  dosage = '';
  frequency = '';
  duration = '';
  rxNotes = '';

  // New Follow-Up Form Fields
  followUpDate = '';
  followUpTime = '09:00 AM';
  availableTimes = ['09:00 AM', '09:45 AM', '10:30 AM', '11:15 AM', '01:00 PM', '01:45 PM', '02:30 PM', '03:15 PM', '04:00 PM'];

  ngOnInit() {
    const dentist = this.authService.currentUser();
    if (!dentist) return;

    this.loadDentistData(dentist.id);
  }

  loadDentistData(dentistId: string) {
    this.appointments = this.dataService.getAppointments().filter(
      a => a.dentistId === dentistId
    );
    this.patients = this.authService.getUsers().filter(u => u.role === 'patient');
  }

  selectPatient(patient: User) {
    this.selectedPatient = patient;
    this.selectedPatientHistory = this.dataService.getMedicalHistory(patient.id);
    this.selectedPatientRecords = this.dataService.getDentalRecords(patient.id);
    this.selectedPatientPrescriptions = this.dataService.getPrescriptions(patient.id);
    this.activeTab = 'patients';
  }

  closePatientDetails() {
    this.selectedPatient = null;
    this.selectedPatientHistory = null;
    this.selectedPatientRecords = [];
    this.selectedPatientPrescriptions = [];
  }

  // Record methods
  openRecordModal() {
    this.showRecordModal = true;
  }

  closeRecordModal() {
    this.showRecordModal = false;
    this.diagnosis = '';
    this.procedurePerformed = '';
    this.clinicalNotes = '';
    this.treatmentPlan = '';
    this.uploadedFile = '';
  }

  handleFileUpload(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.uploadedFile = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  submitRecord() {
    if (!this.selectedPatient || !this.diagnosis || !this.procedurePerformed) {
      alert('Please fill out diagnosis and procedure details.');
      return;
    }

    const dentist = this.authService.currentUser();
    if (!dentist) return;

    const newRecord: DentalRecord = {
      id: 'REC-' + Math.floor(100 + Math.random() * 900),
      patientId: this.selectedPatient.id,
      patientName: this.selectedPatient.name,
      dentistId: dentist.id,
      dentistName: dentist.name,
      date: new Date().toISOString().split('T')[0],
      diagnosis: this.diagnosis,
      procedurePerformed: this.procedurePerformed,
      clinicalNotes: this.clinicalNotes,
      treatmentPlan: this.treatmentPlan,
      files: this.uploadedFile ? [this.uploadedFile] : []
    };

    this.dataService.addDentalRecord(newRecord);
    
    // Refresh details
    this.selectedPatientRecords = this.dataService.getDentalRecords(this.selectedPatient.id);
    
    // Mark any active upcoming appointment as completed if it matches today
    const activeApp = this.appointments.find(
      a => a.patientId === this.selectedPatient?.id && a.status === 'upcoming'
    );
    if (activeApp) {
      this.dataService.updateAppointmentStatus(activeApp.id, 'completed');
      this.loadDentistData(dentist.id);
    }

    this.closeRecordModal();
    alert('Clinical record added successfully.');
  }

  // Prescription methods
  openPrescriptionModal() {
    this.showPrescriptionModal = true;
  }

  closePrescriptionModal() {
    this.showPrescriptionModal = false;
    this.medicationName = '';
    this.dosage = '';
    this.frequency = '';
    this.duration = '';
    this.rxNotes = '';
  }

  submitPrescription() {
    if (!this.selectedPatient || !this.medicationName || !this.dosage || !this.frequency || !this.duration) {
      alert('Please fill out all prescription fields.');
      return;
    }

    const dentist = this.authService.currentUser();
    if (!dentist) return;

    const newPrescription: Prescription = {
      id: 'RX-' + Math.floor(100 + Math.random() * 900),
      patientId: this.selectedPatient.id,
      patientName: this.selectedPatient.name,
      dentistId: dentist.id,
      dentistName: dentist.name,
      date: new Date().toISOString().split('T')[0],
      medicationName: this.medicationName,
      dosage: this.dosage,
      frequency: this.frequency,
      duration: this.duration,
      notes: this.rxNotes
    };

    this.dataService.addPrescription(newPrescription);

    // Refresh details
    this.selectedPatientPrescriptions = this.dataService.getPrescriptions(this.selectedPatient.id);

    this.closePrescriptionModal();
    alert('Prescription issued successfully.');
  }

  // Follow-Up methods
  openFollowUpModal() {
    this.showFollowUpModal = true;
  }

  closeFollowUpModal() {
    this.showFollowUpModal = false;
    this.followUpDate = '';
    this.followUpTime = '09:00 AM';
  }

  submitFollowUp() {
    if (!this.selectedPatient || !this.followUpDate || !this.followUpTime) {
      alert('Please select both date and time.');
      return;
    }

    const dentist = this.authService.currentUser();
    if (!dentist) return;

    const dateObj = new Date(this.followUpDate);
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const formattedDate = `${months[dateObj.getMonth()]} ${dateObj.getDate().toString().padStart(2, '0')}`;

    const newApp: Appointment = {
      id: 'DC-' + Math.floor(100000 + Math.random() * 900000),
      patientId: this.selectedPatient.id,
      patientName: this.selectedPatient.name,
      patientPhone: this.selectedPatient.phone || '',
      serviceId: 'follow-up',
      serviceName: 'Follow-Up Dental Visit',
      price: 0,
      dateStr: formattedDate,
      time: this.followUpTime,
      dentistId: dentist.id,
      dentistName: dentist.name,
      status: 'upcoming'
    };

    this.dataService.saveAppointment(newApp);
    this.loadDentistData(dentist.id);

    this.closeFollowUpModal();
    alert('Follow-up appointment scheduled successfully.');
  }
}
