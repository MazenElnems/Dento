import { Component, OnInit, inject, Input } from '@angular/core';
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

  @Input() activeSubTab = 'dashboard';

  // State collections
  appointments: Appointment[] = [];
  patients: User[] = [];
  selectedPatient: User | null = null;
  selectedPatientHistory: any = null;
  selectedPatientRecords: DentalRecord[] = [];
  selectedPatientPrescriptions: Prescription[] = [];

  // Form patient selection (used when creating a record/rx directly from general lists)
  selectedPatientId = '';
  isDirectBooking = false;

  // Metrics counters
  todayAppointmentsCount = 0;
  upcomingCount = 0;
  completedTodayCount = 0;
  pendingFollowUpsCount = 0;

  // Forms states
  showRecordModal = false;
  showPrescriptionModal = false;
  showFollowUpModal = false;
  showUploadModal = false;

  // Record Form Fields
  diagnosis = '';
  procedurePerformed = '';
  clinicalNotes = '';
  treatmentPlan = '';
  uploadedFile = '';
  uploadedFileName = '';

  // Prescription Form Fields
  medicationName = '';
  dosage = '';
  frequency = '';
  duration = '';
  rxNotes = '';

  // Follow-Up Form Fields
  followUpDate = '';
  followUpTime = '09:00 AM';
  availableTimes = ['09:00 AM', '09:30 AM', '10:00 AM', '10:30 AM', '11:00 AM', '11:30 AM', '01:00 PM', '01:30 PM', '02:00 PM', '02:30 PM', '03:00 PM', '03:30 PM', '04:00 PM'];
  followUpNotes = '';

  // Clinical Files list
  clinicalFiles: { id: string; patientName: string; fileName: string; type: string; date: string; dataUrl: string }[] = [];

  // Mock Storage stats
  storagePercent = 75;

  ngOnInit() {
    const dentist = this.authService.currentUser();
    if (!dentist) return;
    this.loadDentistData(dentist.id);
  }

  loadDentistData(dentistId: string) {
    const allApps = this.dataService.getAppointments();
    
    // Filter dentist's own appointments
    this.appointments = allApps.filter(
      a => a.dentistId === dentistId || a.dentistName.includes('مازن')
    );

    // Patients list
    this.patients = this.authService.getUsers().filter(u => u.role === 'patient');

    // Aggregate stats
    this.todayAppointmentsCount = this.appointments.length;
    this.upcomingCount = this.appointments.filter(a => a.status === 'upcoming').length;
    this.completedTodayCount = this.appointments.filter(a => a.status === 'completed').length;
    this.pendingFollowUpsCount = this.appointments.filter(a => a.serviceId === 'follow-up' && a.status === 'upcoming').length;

    // Load mock clinical files
    this.loadClinicalFiles();
  }

  loadClinicalFiles() {
    const stored = localStorage.getItem('dc_clinical_files');
    if (stored) {
      this.clinicalFiles = JSON.parse(stored);
    } else {
      // Seed some initial files
      const seedFiles = [
        { id: 'FL-01', patientName: 'أحمد علي', fileName: 'X-Ray Panoramic.jpg', type: 'X-ray', date: '2026-07-08', dataUrl: 'https://images.unsplash.com/photo-1579684389782-64d84b5e901d?w=400' },
        { id: 'FL-02', patientName: 'سارة محمد', fileName: 'Teeth Before Treatment.png', type: 'Before Image', date: '2026-07-09', dataUrl: 'https://images.unsplash.com/photo-1598256989800-fe5f95da9787?w=400' }
      ];
      localStorage.setItem('dc_clinical_files', JSON.stringify(seedFiles));
      this.clinicalFiles = seedFiles;
    }
  }

  selectPatient(patient: User) {
    this.selectedPatient = patient;
    this.selectedPatientHistory = this.dataService.getMedicalHistory(patient.id) || {
      bloodPressure: '120/80',
      diabetes: 'No',
      heartDisease: 'No',
      allergies: 'None',
      currentMedications: 'None',
      previousSurgeries: 'None',
      smokingStatus: 'Non-Smoker',
      additionalNotes: 'Patient has healthy teeth structure.'
    };
    this.selectedPatientRecords = this.dataService.getDentalRecords(patient.id);
    this.selectedPatientPrescriptions = this.dataService.getPrescriptions(patient.id);
    this.activeSubTab = 'patients';
  }

  closePatientDetails() {
    this.selectedPatient = null;
    this.selectedPatientHistory = null;
    this.selectedPatientRecords = [];
    this.selectedPatientPrescriptions = [];
  }

  // Modals Toggles
  openRecordModal(isDirect = false) {
    this.isDirectBooking = isDirect;
    this.showRecordModal = true;
  }
  closeRecordModal() {
    this.showRecordModal = false;
    this.diagnosis = '';
    this.procedurePerformed = '';
    this.clinicalNotes = '';
    this.treatmentPlan = '';
    this.uploadedFile = '';
    this.uploadedFileName = '';
    this.selectedPatientId = '';
    this.isDirectBooking = false;
  }

  openPrescriptionModal(isDirect = false) {
    this.isDirectBooking = isDirect;
    this.showPrescriptionModal = true;
  }
  closePrescriptionModal() {
    this.showPrescriptionModal = false;
    this.medicationName = '';
    this.dosage = '';
    this.frequency = '';
    this.duration = '';
    this.rxNotes = '';
    this.selectedPatientId = '';
    this.isDirectBooking = false;
  }

  openFollowUpModal(isDirect = false) {
    this.isDirectBooking = isDirect;
    this.showFollowUpModal = true;
  }
  closeFollowUpModal() {
    this.showFollowUpModal = false;
    this.followUpDate = '';
    this.followUpTime = '09:00 AM';
    this.followUpNotes = '';
    this.selectedPatientId = '';
    this.isDirectBooking = false;
  }

  openUploadModal(isDirect = false) {
    this.isDirectBooking = isDirect;
    this.showUploadModal = true;
  }
  closeUploadModal() {
    this.showUploadModal = false;
    this.uploadedFile = '';
    this.uploadedFileName = '';
    this.selectedPatientId = '';
    this.isDirectBooking = false;
  }

  // File Upload Handlers
  handleFileUpload(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.uploadedFileName = file.name;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.uploadedFile = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  saveClinicalFile() {
    let targetPatient = null;
    if (this.isDirectBooking) {
      if (this.selectedPatientId) {
        targetPatient = this.patients.find(p => p.id === this.selectedPatientId) || null;
      }
    } else {
      targetPatient = this.selectedPatient;
    }

    if (!targetPatient || !this.uploadedFile) {
      alert('Please select a patient and upload a file first.');
      return;
    }

    const newFile = {
      id: 'FL-' + Math.floor(100 + Math.random() * 900),
      patientName: targetPatient.name,
      fileName: this.uploadedFileName || 'Document.pdf',
      type: this.uploadedFileName.toLowerCase().endsWith('.pdf') ? 'Supporting Document' : 'X-ray',
      date: new Date().toISOString().split('T')[0],
      dataUrl: this.uploadedFile
    };

    this.clinicalFiles.push(newFile);
    localStorage.setItem('dc_clinical_files', JSON.stringify(this.clinicalFiles));
    
    // Increase storage progress slightly
    this.storagePercent = Math.min(100, this.storagePercent + 2);
    
    this.closeUploadModal();
    alert('Clinical document uploaded and linked successfully.');
  }

  // Action Form Submissions
  submitRecord() {
    let targetPatient = null;
    if (this.isDirectBooking) {
      if (this.selectedPatientId) {
        targetPatient = this.patients.find(p => p.id === this.selectedPatientId) || null;
      }
    } else {
      targetPatient = this.selectedPatient;
    }

    if (!targetPatient || !this.diagnosis || !this.procedurePerformed) {
      alert('Please fill out diagnosis and procedure fields.');
      return;
    }

    const dentist = this.authService.currentUser();
    if (!dentist) return;

    const newRecord: DentalRecord = {
      id: 'REC-' + Math.floor(100 + Math.random() * 900),
      patientId: targetPatient.id,
      patientName: targetPatient.name,
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
    if (this.selectedPatient) {
      this.selectedPatientRecords = this.dataService.getDentalRecords(this.selectedPatient.id);
    }
    
    // Mark today's appointment as completed
    const activeApp = this.appointments.find(
      a => a.patientId === targetPatient.id && a.status === 'upcoming'
    );
    if (activeApp) {
      this.dataService.updateAppointmentStatus(activeApp.id, 'completed');
      this.loadDentistData(dentist.id);
    }

    this.closeRecordModal();
    alert('Clinical record created successfully.');
  }

  submitPrescription() {
    let targetPatient = null;
    if (this.isDirectBooking) {
      if (this.selectedPatientId) {
        targetPatient = this.patients.find(p => p.id === this.selectedPatientId) || null;
      }
    } else {
      targetPatient = this.selectedPatient;
    }

    if (!targetPatient) {
      alert('Please select a patient.');
      return;
    }

    if (!this.medicationName || !this.dosage || !this.frequency || !this.duration) {
      alert('Please fill out all prescription fields.');
      return;
    }

    const dentist = this.authService.currentUser();
    if (!dentist) return;

    const newPrescription: Prescription = {
      id: 'RX-' + Math.floor(100 + Math.random() * 900),
      patientId: targetPatient.id,
      patientName: targetPatient.name,
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
    if (this.selectedPatient) {
      this.selectedPatientPrescriptions = this.dataService.getPrescriptions(this.selectedPatient.id);
    }

    this.closePrescriptionModal();
    alert('Prescription issued successfully.');
  }

  submitFollowUp() {
    let targetPatient = null;
    if (this.isDirectBooking) {
      if (this.selectedPatientId) {
        targetPatient = this.patients.find(p => p.id === this.selectedPatientId) || null;
      }
    } else {
      targetPatient = this.selectedPatient;
    }

    if (!targetPatient || !this.followUpDate || !this.followUpTime) {
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
      patientId: targetPatient.id,
      patientName: targetPatient.name,
      patientPhone: targetPatient.phone || '',
      serviceId: 'follow-up',
      serviceName: 'Follow-Up Dental Visit',
      price: 60,
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
