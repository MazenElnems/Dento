import { Component, OnInit, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../services/auth.service';
import { DataService, Appointment, DentistVacation, LeaveRequest } from '../../services/data.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  authService = inject(AuthService);
  dataService = inject(DataService);

  // Sub-Navigation Tabs (Driven by master sidebar)
  @Input() activeSubTab: 'dashboard' | 'users' | 'schedules' | 'revenue' | 'reports' = 'dashboard';
  activeUserTab: 'patients' | 'dentists' | 'receptionists' = 'patients';
  calendarView: 'day' | 'week' | 'month' = 'month';

  // Raw collections
  users: any[] = [];
  appointments: Appointment[] = [];
  vacations: DentistVacation[] = [];
  leaveRequests: LeaveRequest[] = [];

  // Filtered collections
  patients: any[] = [];
  dentists: any[] = [];
  receptionists: any[] = [];

  // Stats
  totalPatientsCount = 0;
  totalDentistsCount = 0;
  totalReceptionistsCount = 0;
  totalAppointmentsCount = 0;
  completedAppointmentsCount = 0;
  cancelledAppointmentsCount = 0;
  revenueToday = 0;
  revenueMonth = 0;
  dentistUtilizationRate = 82; // Real utilization mockup

  // Modals visibility
  showUserModal = false;
  showScheduleConfigModal = false;
  showVacationModal = false;

  // Selected entities for edit/view
  selectedUserForEdit: any = null;
  selectedDentistForSchedule: any = null;

  // Form Fields: User CRUD
  userFormRole: 'patient' | 'dentist' | 'receptionist' = 'patient';
  userFormFirstName = '';
  userFormLastName = '';
  userFormEmail = '';
  userFormPassword = '';
  userFormPhone = '';
  userFormGender: 'Male' | 'Female' = 'Male';
  userFormBirthDate = '';
  userFormSpecialty = '';

  // Form Fields: Schedule Configuration
  scheduleSat = true;
  scheduleSun = true;
  scheduleMon = true;
  scheduleTue = true;
  scheduleWed = true;
  scheduleThu = true;
  scheduleFri = false;
  scheduleFromHour = '09:00';
  scheduleToHour = '17:00';
  scheduleSlotLength = 30;

  // Form Fields: Vacation Modal
  vacationDentistEmail = '';
  vacationStartDate = '';
  vacationEndDate = '';

  // Weekly Revenue dataset for Chart display
  weeklyRevenueData = [
    { label: 'Week 1', value: 450, percentage: '45%' },
    { label: 'Week 2', value: 600, percentage: '60%' },
    { label: 'Week 3', value: 850, percentage: '85%' },
    { label: 'Week 4', value: 1200, percentage: '100%' }
  ];

  // Calendar dates mock generator for Schedule view
  calendarDays: number[] = [];

  ngOnInit() {
    this.loadAllData();
    this.generateCalendarDays();
  }

  loadAllData() {
    this.users = this.authService.getUsers();
    this.appointments = this.dataService.getAppointments();
    this.vacations = this.dataService.getVacations();
    this.leaveRequests = this.dataService.getLeaveRequests();

    // Categorize
    this.dentists = this.users.filter(u => u.role === 'dentist' || u.role === 'admin');
    this.receptionists = this.users.filter(u => u.role === 'receptionist');
    
    // Auto-seed some patients if empty
    const localPatients = this.users.filter(u => u.role === 'patient');
    if (localPatients.length === 0) {
      const seedPatients = [
        { id: 'pat_01', name: 'أكرم شوايل', email: 'akramshwail288@gmail.com', role: 'patient', phone: '01002930219', status: 'active' },
        { id: 'pat_02', name: 'أحمد محمود', email: 'ahmed@gmail.com', role: 'patient', phone: '01129384756', status: 'active' },
        { id: 'pat_03', name: 'ليلى كريم', email: 'layla@gmail.com', role: 'patient', phone: '01293847561', status: 'active' },
        { id: 'pat_04', name: 'ياسر خالد', email: 'yasser@gmail.com', role: 'patient', phone: '01029384756', status: 'inactive' }
      ];
      seedPatients.forEach(p => this.dataService.addUser(p));
      this.patients = seedPatients;
    } else {
      this.patients = localPatients;
    }

    // Refresh count values
    this.totalPatientsCount = this.patients.length;
    this.totalDentistsCount = this.dentists.length;
    this.totalReceptionistsCount = this.receptionists.length;
    this.totalAppointmentsCount = this.appointments.length;
    
    const completedList = this.appointments.filter(a => a.status === 'completed');
    this.completedAppointmentsCount = completedList.length;
    this.cancelledAppointmentsCount = this.appointments.filter(a => a.status === 'cancelled').length;

    // Calculate revenue
    this.revenueMonth = completedList.reduce((acc, app) => acc + (app.price || 0), 0);
    this.revenueToday = Math.round(this.revenueMonth * 0.12);
  }

  generateCalendarDays() {
    this.calendarDays = Array.from({ length: 30 }, (_, i) => i + 1);
  }

  // Toggle user activation status
  toggleStatus(user: any) {
    const nextStatus = user.status === 'inactive' ? 'active' : 'inactive';
    this.dataService.updateUserStatus(user.id, nextStatus);
    alert(`Account has been ${nextStatus}d successfully.`);
    this.loadAllData();
  }

  // Leave approval actions
  approveLeave(leave: LeaveRequest) {
    this.dataService.updateLeaveStatus(leave.id, 'approved');
    alert('Leave request approved successfully.');
    this.loadAllData();
  }

  rejectLeave(leave: LeaveRequest) {
    this.dataService.updateLeaveStatus(leave.id, 'rejected');
    alert('Leave request rejected.');
    this.loadAllData();
  }

  // Delete vacation slot
  deleteVacation(vacId: string) {
    const confirmDel = confirm('Are you sure you want to remove this vacation period?');
    if (confirmDel) {
      this.dataService.deleteVacation(vacId);
      this.loadAllData();
    }
  }

  // Switch tabs
  switchTab(tab: typeof AdminDashboard.prototype.activeSubTab) {
    this.activeSubTab = tab;
  }

  switchUserTab(subTab: typeof AdminDashboard.prototype.activeUserTab) {
    this.activeUserTab = subTab;
  }

  // Create User trigger
  openCreateUserModal(role: typeof AdminDashboard.prototype.userFormRole) {
    this.selectedUserForEdit = null;
    this.userFormRole = role;
    
    // Reset form fields
    this.userFormFirstName = '';
    this.userFormLastName = '';
    this.userFormEmail = '';
    this.userFormPassword = '';
    this.userFormPhone = '';
    this.userFormGender = 'Male';
    this.userFormBirthDate = '';
    this.userFormSpecialty = '';

    this.showUserModal = true;
  }

  // Edit User trigger
  openEditUserModal(user: any) {
    this.selectedUserForEdit = user;
    this.userFormRole = user.role;
    
    // Split name
    const parts = user.name.split(' ');
    this.userFormFirstName = parts[0] || '';
    this.userFormLastName = parts.slice(1).join(' ') || '';
    this.userFormEmail = user.email;
    this.userFormPassword = ''; // Stay empty unless modifying
    this.userFormPhone = user.phone || '';
    this.userFormGender = user.gender || 'Male';
    this.userFormBirthDate = user.birthDate || '';
    this.userFormSpecialty = user.specialty || '';

    this.showUserModal = true;
  }

  closeUserModal() {
    this.showUserModal = false;
    this.selectedUserForEdit = null;
  }

  saveUser() {
    if (!this.userFormFirstName || !this.userFormLastName || !this.userFormEmail) {
      alert('Please fill out all required name and email fields.');
      return;
    }

    const fullName = `${this.userFormFirstName} ${this.userFormLastName}`;

    if (this.selectedUserForEdit) {
      // Modify
      const updatedUser = {
        ...this.selectedUserForEdit,
        name: fullName,
        email: this.userFormEmail,
        phone: this.userFormPhone,
        gender: this.userFormGender,
        birthDate: this.userFormBirthDate,
        specialty: this.userFormSpecialty
      };
      this.dataService.updateUser(updatedUser);
      alert('User details updated successfully.');
      this.loadAllData();
      this.closeUserModal();
    } else {
      // Create New User
      const newId = this.userFormRole + '_' + Math.floor(1000 + Math.random() * 9000);
      const newUser = {
        id: newId,
        name: fullName,
        email: this.userFormEmail,
        role: this.userFormRole,
        phone: this.userFormPhone,
        gender: this.userFormGender,
        birthDate: this.userFormBirthDate,
        specialty: this.userFormSpecialty,
        status: 'active'
      };
      this.dataService.addUser(newUser);
      alert(`${this.userFormRole.toUpperCase()} created successfully!`);
      this.loadAllData();
      this.closeUserModal();
    }
  }

  // Schedule Config Methods
  openScheduleConfigModal(dentist: any) {
    this.selectedDentistForSchedule = dentist;
    
    // Load existing schedule config
    const list = this.dataService.getDentistSchedules();
    const existing = list.find((s: any) => s.email === dentist.email);
    if (existing) {
      this.scheduleFromHour = existing.hours.split(' - ')[0] || '09:00';
      this.scheduleToHour = existing.hours.split(' - ')[1] || '17:00';
      this.scheduleSlotLength = existing.slotLength || 30;
      this.scheduleSat = existing.workingDays.includes(6);
      this.scheduleSun = existing.workingDays.includes(0);
      this.scheduleMon = existing.workingDays.includes(1);
      this.scheduleTue = existing.workingDays.includes(2);
      this.scheduleWed = existing.workingDays.includes(3);
      this.scheduleThu = existing.workingDays.includes(4);
      this.scheduleFri = existing.workingDays.includes(5);
    }

    this.showScheduleConfigModal = true;
  }

  closeScheduleConfigModal() {
    this.showScheduleConfigModal = false;
    this.selectedDentistForSchedule = null;
  }

  saveScheduleConfig() {
    if (!this.selectedDentistForSchedule) return;

    const workingDays: number[] = [];
    if (this.scheduleSun) workingDays.push(0);
    if (this.scheduleMon) workingDays.push(1);
    if (this.scheduleTue) workingDays.push(2);
    if (this.scheduleWed) workingDays.push(3);
    if (this.scheduleThu) workingDays.push(4);
    if (this.scheduleFri) workingDays.push(5);
    if (this.scheduleSat) workingDays.push(6);

    const list = this.dataService.getDentistSchedules();
    const index = list.findIndex((s: any) => s.email === this.selectedDentistForSchedule?.email);
    
    const newSchedule = {
      email: this.selectedDentistForSchedule.email,
      name: this.selectedDentistForSchedule.name,
      workingDays,
      hours: `${this.scheduleFromHour} - ${this.scheduleToHour}`,
      slotLength: this.scheduleSlotLength
    };

    if (index >= 0) {
      list[index] = newSchedule;
    } else {
      list.push(newSchedule);
    }

    this.dataService.saveDentistSchedules(list);
    this.closeScheduleConfigModal();
    alert('Dentist schedule saved.');
  }

  // Vacation triggers
  openVacationModal() {
    if (this.dentists.length > 0) {
      this.vacationDentistEmail = this.dentists[0].email;
    }
    this.vacationStartDate = '';
    this.vacationEndDate = '';
    this.showVacationModal = true;
  }

  closeVacationModal() {
    this.showVacationModal = false;
  }

  saveVacation() {
    if (!this.vacationDentistEmail || !this.vacationStartDate || !this.vacationEndDate) {
      alert('Please fill out all vacation date fields.');
      return;
    }

    const dentist = this.dentists.find(d => d.email === this.vacationDentistEmail);
    if (!dentist) return;

    const newVacation: DentistVacation = {
      id: 'VAC-' + Math.floor(100 + Math.random() * 900),
      dentistEmail: dentist.email,
      dentistName: dentist.name,
      startDate: this.vacationStartDate,
      endDate: this.vacationEndDate
    };

    this.dataService.saveVacation(newVacation);
    alert('Vacation period scheduled.');
    this.loadAllData();
    this.closeVacationModal();
  }
}
