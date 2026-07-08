import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, User } from '../../services/auth.service';
import { DataService, Appointment } from '../../services/data.service';

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

  // Lists
  users: User[] = [];
  appointments: Appointment[] = [];
  dentists: User[] = [];
  receptionists: User[] = [];
  patients: User[] = [];

  // Tabs
  activeTab: 'analytics' | 'users' | 'schedules' = 'analytics';

  // Stats
  totalRevenue = 0;
  dailyRevenue = 0;
  weeklyRevenue = 0;
  monthlyRevenue = 0;
  completedAppCount = 0;
  cancelledAppCount = 0;
  utilizationRate = 78; // Mock clinic efficiency rate

  // Staff Modal
  showStaffModal = false;
  staffType: 'dentist' | 'receptionist' = 'dentist';
  
  // Staff Form fields
  staffFirstName = '';
  staffMiddleName = '';
  staffLastName = '';
  staffEmail = '';
  staffPassword = '';
  staffPhone = '';
  staffGender: 'Male' | 'Female' = 'Male';
  staffBirthDate = '';
  staffSpecialty = '';

  // Schedule Config Modal
  showScheduleModal = false;
  selectedDentistForSchedule: User | null = null;
  
  // Schedule Form fields
  sat = true;
  sun = true;
  mon = true;
  tue = true;
  wed = true;
  thu = true;
  fri = false;
  fromHour = '09:00';
  toHour = '17:00';
  slotLength = 30;

  ngOnInit() {
    this.loadAdminData();
  }

  loadAdminData() {
    this.users = this.authService.getUsers();
    this.appointments = this.dataService.getAppointments();

    this.dentists = this.users.filter(u => u.role === 'dentist' || u.role === 'admin');
    this.receptionists = this.users.filter(u => u.role === 'receptionist');
    this.patients = this.users.filter(u => u.role === 'patient');

    // Calculate analytics metrics
    const completedApps = this.appointments.filter(a => a.status === 'completed');
    this.completedAppCount = completedApps.length;
    this.cancelledAppCount = this.appointments.filter(a => a.status === 'cancelled').length;

    // Total revenue sum
    this.totalRevenue = completedApps.reduce((acc, current) => acc + (current.price || 0), 0);
    this.dailyRevenue = Math.round(this.totalRevenue * 0.15);
    this.weeklyRevenue = Math.round(this.totalRevenue * 0.45);
    this.monthlyRevenue = this.totalRevenue;
  }

  toggleUserStatus(user: User) {
    const newStatus = user.status === 'inactive' ? 'active' : 'inactive';
    this.dataService.updateUserStatus(user.id, newStatus);
    
    // Alert and refresh
    alert(`User account has been ${newStatus}d successfully.`);
    this.loadAdminData();
  }

  openStaffModal(type: 'dentist' | 'receptionist') {
    this.staffType = type;
    this.showStaffModal = true;
  }

  closeStaffModal() {
    this.showStaffModal = false;
    this.staffFirstName = '';
    this.staffMiddleName = '';
    this.staffLastName = '';
    this.staffEmail = '';
    this.staffPassword = '';
    this.staffPhone = '';
    this.staffBirthDate = '';
    this.staffSpecialty = '';
  }

  submitStaff() {
    if (!this.staffFirstName || !this.staffLastName || !this.staffEmail || !this.staffPassword) {
      alert('Please fill out all required staff details.');
      return;
    }

    if (this.staffType === 'dentist') {
      if (!this.staffSpecialty) {
        alert('Please specify the dentist specialty.');
        return;
      }
      this.authService.registerDentist({
        firstName: this.staffFirstName,
        middleName: this.staffMiddleName || 'Staff',
        lastName: this.staffLastName,
        email: this.staffEmail,
        password: this.staffPassword,
        specialty: this.staffSpecialty
      }).subscribe({
        next: (response) => {
          if (response.success) {
            // Mock local insertion to sync admin directory immediately
            const mockUser: User = {
              id: 'dentist_' + Math.floor(1000 + Math.random() * 9000),
              name: `${this.staffFirstName} ${this.staffLastName}`,
              email: this.staffEmail,
              role: 'dentist',
              status: 'active'
            };
            this.dataService.addUser({ ...mockUser, password: this.staffPassword });
            
            alert('Dentist registered successfully!');
            this.loadAdminData();
            this.closeStaffModal();
          } else {
            alert(response.message || 'Failed to register dentist.');
          }
        },
        error: (err) => {
          // Fallback to local simulator
          console.error(err);
          const mockUser: User = {
            id: 'dentist_' + Math.floor(1000 + Math.random() * 9000),
            name: `${this.staffFirstName} ${this.staffLastName}`,
            email: this.staffEmail,
            role: 'dentist',
            status: 'active'
          };
          this.dataService.addUser({ ...mockUser, password: this.staffPassword });
          alert('Staff registered locally (Simulated success).');
          this.loadAdminData();
          this.closeStaffModal();
        }
      });
    } else {
      if (!this.staffPhone || !this.staffBirthDate) {
        alert('Please fill phone number and birth date.');
        return;
      }
      this.authService.registerReceptionist({
        firstName: this.staffFirstName,
        middleName: this.staffMiddleName || 'Staff',
        lastName: this.staffLastName,
        phone: this.staffPhone,
        gender: this.staffGender,
        birthDate: this.staffBirthDate,
        email: this.staffEmail,
        password: this.staffPassword
      }).subscribe({
        next: (response) => {
          if (response.success) {
            const mockUser: User = {
              id: 'recep_' + Math.floor(1000 + Math.random() * 9000),
              name: `${this.staffFirstName} ${this.staffLastName}`,
              email: this.staffEmail,
              role: 'receptionist',
              phone: this.staffPhone,
              status: 'active'
            };
            this.dataService.addUser({ ...mockUser, password: this.staffPassword });
            
            alert('Receptionist registered successfully!');
            this.loadAdminData();
            this.closeStaffModal();
          } else {
            alert(response.message || 'Failed to register receptionist.');
          }
        },
        error: (err) => {
          console.error(err);
          const mockUser: User = {
            id: 'recep_' + Math.floor(1000 + Math.random() * 9000),
            name: `${this.staffFirstName} ${this.staffLastName}`,
            email: this.staffEmail,
            role: 'receptionist',
            phone: this.staffPhone,
            status: 'active'
          };
          this.dataService.addUser({ ...mockUser, password: this.staffPassword });
          alert('Staff registered locally (Simulated success).');
          this.loadAdminData();
          this.closeStaffModal();
        }
      });
    }
  }

  // Schedule Config Methods
  openScheduleModal(dentist: User) {
    this.selectedDentistForSchedule = dentist;
    
    // Load existing config if exists
    const list = this.dataService.getDentistSchedules();
    const existing = list.find((s: any) => s.email === dentist.email);
    if (existing) {
      this.fromHour = existing.hours.split(' - ')[0] || '09:00';
      this.toHour = existing.hours.split(' - ')[1] || '17:00';
      this.sat = existing.workingDays.includes(6);
      this.sun = existing.workingDays.includes(0);
      this.mon = existing.workingDays.includes(1);
      this.tue = existing.workingDays.includes(2);
      this.wed = existing.workingDays.includes(3);
      this.thu = existing.workingDays.includes(4);
      this.fri = existing.workingDays.includes(5);
    }
    
    this.showScheduleModal = true;
  }

  closeScheduleModal() {
    this.showScheduleModal = false;
    this.selectedDentistForSchedule = null;
  }

  submitSchedule() {
    if (!this.selectedDentistForSchedule) return;

    const workingDays: number[] = [];
    if (this.sun) workingDays.push(0);
    if (this.mon) workingDays.push(1);
    if (this.tue) workingDays.push(2);
    if (this.wed) workingDays.push(3);
    if (this.thu) workingDays.push(4);
    if (this.fri) workingDays.push(5);
    if (this.sat) workingDays.push(6);

    const list = this.dataService.getDentistSchedules();
    const index = list.findIndex((s: any) => s.email === this.selectedDentistForSchedule?.email);
    
    const newSchedule = {
      email: this.selectedDentistForSchedule.email,
      name: this.selectedDentistForSchedule.name,
      workingDays,
      hours: `${this.fromHour} - ${this.toHour}`
    };

    if (index >= 0) {
      list[index] = newSchedule;
    } else {
      list.push(newSchedule);
    }

    this.dataService.saveDentistSchedules(list);
    this.closeScheduleModal();
    alert('Dentist work schedule updated successfully.');
  }
}
