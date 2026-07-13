import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { BookingService, Dentist, DaySchedule, Slot } from '../services/booking.service';
import { AuthService } from '../services/auth.service';

interface Service {
  id: string;
  name: string;
  price: number;
  duration: string;
}

interface MappedTimeSlot {
  id: string;
  timeStr: string;
  available: boolean;
}

interface MappedDaySlots {
  dateVal: string; // e.g. "2026-07-15"
  dateStr: string; // e.g. "Jul 15"
  dayName: string; // e.g. "Wednesday"
  slots: MappedTimeSlot[];
}

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './booking.html',
  styleUrl: './booking.css'
})
export class Booking implements OnInit, OnDestroy {
  bookingService = inject(BookingService);
  authService = inject(AuthService);
  router = inject(Router);

  currentStep = 1;

  // Services list
  services: Service[] = [
    { id: 'whitening', name: 'Teeth Whitening', price: 150, duration: '45 mins' },
    { id: 'implants', name: 'Dental Implants', price: 1200, duration: '60 mins' },
    { id: 'ortho', name: 'Orthodontics (Braces)', price: 2500, duration: '45 mins' },
    { id: 'canal', name: 'Root Canal Treatment', price: 350, duration: '60 mins' },
    { id: 'cosmetic', name: 'Cosmetic Dentistry', price: 800, duration: '60 mins' },
    { id: 'cleaning', name: 'Teeth Cleaning & Hygiene', price: 90, duration: '30 mins' }
  ];

  selectedService: Service | null = null;
  
  // Dentists collections from API
  dentistsList: Dentist[] = [];
  selectedDentist: Dentist | null = null;

  // Slots schedule mapped from API
  calendarDays: MappedDaySlots[] = [];
  selectedDay: MappedDaySlots | null = null;
  selectedTimeSlot: MappedTimeSlot | null = null;

  // Form Fields
  patientName = '';
  patientEmail = '';
  patientPhone = '';

  // Appt lock state
  lockedAppointmentId = '';
  countdownMinutes = 10;
  countdownSeconds = 0;
  timerInterval: any = null;

  isSubmitting = false;
  paymentErrorMessage = '';
  paymentMethod: 'card' | 'cash' = 'card';

  setPaymentMethod(method: 'card' | 'cash') {
    this.paymentMethod = method;
  }

  confirmCashBooking() {
    if (!this.lockedAppointmentId) {
      alert('No locked appointment found.');
      return;
    }

    if (this.lockedAppointmentId.startsWith('mock_')) {
      this.clearLockTimer();
      alert('تم تأكيد الحجز (محاكاة)! يرجى دفع قيمة الكشف نقدًا عند الحضور للعيادة.');
      this.router.navigate(['/payment/result'], {
        queryParams: { id: this.lockedAppointmentId, success: 'true', method: 'cash' }
      });
      return;
    }

    this.isSubmitting = true;
    const idempotencyKey = this.generateIdempotencyKey();
    
    this.bookingService.createPayment(this.lockedAppointmentId, idempotencyKey, 'Cash').subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success) {
          this.clearLockTimer();
          alert('تم تأكيد الحجز بنجاح! يرجى دفع قيمة الكشف نقدًا عند الحضور للعيادة.');
          this.router.navigate(['/payment/result'], {
            queryParams: { id: this.lockedAppointmentId, success: 'true', method: 'cash' }
          });
        } else {
          alert(res.message || 'Failed to confirm cash booking.');
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        alert('حدث خطأ أثناء تأكيد الحجز النقدي. يرجى المحاولة مرة أخرى.');
        console.error(err);
      }
    });
  }

  ngOnInit() {
    this.selectedService = this.services[0];
    
    // Auto-populate from logged-in patient
    const user = this.authService.currentUser();
    if (user) {
      this.patientName = user.name;
      this.patientEmail = user.email;
      this.patientPhone = user.phone || '';
    }

    // Load available dentists
    this.loadDentists();
  }

  ngOnDestroy() {
    this.clearLockTimer();
  }

  loadDentists() {
    this.bookingService.getDentists().subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.dentistsList = res.data;
          this.selectDentist(this.dentistsList[0]);
        } else {
          this.useMockDentists();
        }
      },
      error: (err) => {
        console.error('Failed to load dentists:', err);
        this.useMockDentists();
      }
    });
  }

  useMockDentists() {
    let cookieDentists: Dentist[] = [];
    try {
      console.log('booking.ts: Reading document.cookie =', document.cookie);
      const cookies = document.cookie.split(';');
      const userCookie = cookies.find(c => c.trim().startsWith('dc_shared_users='));
      if (userCookie) {
        const rawJson = decodeURIComponent(userCookie.split('=')[1]);
        console.log('booking.ts: Found dc_shared_users raw JSON =', rawJson);
        const users = JSON.parse(rawJson);
        cookieDentists = users.map((d: any) => ({
          id: d.id,
          fullName: d.name,
          specialty: d.specialty || 'General Dentist',
          consultationFee: d.id.includes('admin') || d.id.includes('hana') ? 250 : 350,
          scheduleId: 'mock_schedule_' + d.id,
          imageUrl: d.imageUrl || 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=200',
          yearsOfExperience: d.id.includes('admin') ? 8 : 10
        }));
        console.log('booking.ts: Mapped cookieDentists =', cookieDentists);
      } else {
        console.warn('booking.ts: dc_shared_users cookie not found');
      }
    } catch (e) {
      console.error('Error loading dentists from cookie:', e);
    }

    const defaults = [
      {
        id: 'mock_hana',
        fullName: 'د. هنا بشرى (Lead Orthodontist)',
        specialty: 'Orthodontics (تقويم الأسنان)',
        consultationFee: 250,
        scheduleId: 'mock_schedule_hana',
        imageUrl: 'https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=200',
        yearsOfExperience: 8
      },
      {
        id: 'mock_mazen',
        fullName: 'د. مازن النمس (Implant Specialist)',
        specialty: 'Cosmetic Dentistry (تجميل الأسنان)',
        consultationFee: 350,
        scheduleId: 'mock_schedule_mazen',
        imageUrl: 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=200',
        yearsOfExperience: 10
      }
    ];

    this.dentistsList = [...cookieDentists, ...defaults];
    
    // De-duplicate dentists by ID
    const seen = new Set();
    this.dentistsList = this.dentistsList.filter(el => {
      const duplicate = seen.has(el.id);
      seen.add(el.id);
      return !duplicate;
    });

    this.selectDentist(this.dentistsList[0]);
  }

  selectDentist(dentist: Dentist) {
    this.selectedDentist = dentist;
    this.calendarDays = [];
    this.selectedDay = null;
    this.selectedTimeSlot = null;

    if (dentist.scheduleId.startsWith('mock_')) {
      this.generateMockScheduleSlots();
      return;
    }

    // Fetch this dentist schedule slots
    this.bookingService.getSchedule(dentist.scheduleId).subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.schedule && res.data.schedule.length > 0) {
          this.mapScheduleData(res.data.schedule);
        } else {
          this.generateMockScheduleSlots();
        }
      },
      error: (err) => {
        console.error('Failed to load schedule for dentist:', err);
        this.generateMockScheduleSlots();
      }
    });
  }

  generateMockScheduleSlots() {
    const days: MappedDaySlots[] = [];
    const today = new Date();
    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    for (let i = 1; i <= 14; i++) {
      const nextDate = new Date(today);
      nextDate.setDate(today.getDate() + i);

      // Skip Fridays
      if (nextDate.getDay() === 5) continue;

      const dayName = dayNames[nextDate.getDay()];
      const dateStr = nextDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      const dateVal = nextDate.toISOString().split('T')[0];

      const slots: MappedTimeSlot[] = [
        { id: `mock_slot_${i}_1`, timeStr: '09:00 AM - 09:45 AM', available: true },
        { id: `mock_slot_${i}_2`, timeStr: '10:30 AM - 11:15 AM', available: true },
        { id: `mock_slot_${i}_3`, timeStr: '01:00 PM - 01:45 PM', available: true },
        { id: `mock_slot_${i}_4`, timeStr: '03:15 PM - 04:00 PM', available: true }
      ];

      days.push({
        dateVal,
        dateStr,
        dayName,
        slots
      });
    }

    this.calendarDays = days;
    if (this.calendarDays.length > 0) {
      this.selectedDay = this.calendarDays[0];
    }
  }

  mapScheduleData(schedule: DaySchedule[]) {
    const days: MappedDaySlots[] = [];
    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    schedule.forEach(day => {
      const dateObj = new Date(day.date);
      const dayName = dayNames[dateObj.getDay()];
      const dateStr = dateObj.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });

      const mappedSlots = day.slots.map(slot => ({
        id: slot.id,
        timeStr: this.formatTime(slot.from) + ' - ' + this.formatTime(slot.to),
        available: true // API only returns available slots
      }));

      if (mappedSlots.length > 0) {
        days.push({
          dateVal: day.date,
          dateStr,
          dayName,
          slots: mappedSlots
        });
      }
    });

    this.calendarDays = days;
    if (this.calendarDays.length > 0) {
      this.selectedDay = this.calendarDays[0];
    }
  }

  formatTime(timeStr: string): string {
    const parts = timeStr.split(':');
    let hour = parseInt(parts[0], 10);
    const minute = parts[1];
    const ampm = hour >= 12 ? 'PM' : 'AM';
    hour = hour % 12;
    hour = hour ? hour : 12;
    return `${hour}:${minute} ${ampm}`;
  }

  selectService(service: Service) {
    this.selectedService = service;
  }

  selectDay(day: MappedDaySlots) {
    this.selectedDay = day;
    this.selectedTimeSlot = null;
  }

  selectTime(slot: MappedTimeSlot) {
    if (slot.available) {
      this.selectedTimeSlot = slot;
    }
  }

  // Timer utilities
  startLockTimer() {
    this.clearLockTimer();
    this.countdownMinutes = 10;
    this.countdownSeconds = 0;

    this.timerInterval = setInterval(() => {
      if (this.countdownSeconds > 0) {
        this.countdownSeconds--;
      } else if (this.countdownMinutes > 0) {
        this.countdownMinutes--;
        this.countdownSeconds = 59;
      } else {
        this.clearLockTimer();
        alert('انتهت مهلة الحجز البالغة 10 دقائق. يرجى اختيار موعد جديد.');
        this.goToStep(1);
      }
    }, 1000);
  }

  clearLockTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  get formattedTimeLeft(): string {
    const mins = this.countdownMinutes.toString().padStart(2, '0');
    const secs = this.countdownSeconds.toString().padStart(2, '0');
    return `${mins}:${secs}`;
  }

  goToStep(step: number) {
    if (step === 2) {
      if (!this.selectedService) {
        alert('Please select a service.');
        return;
      }
      if (!this.selectedDentist) {
        alert('Please select a dentist.');
        return;
      }
      if (!this.selectedDay || !this.selectedTimeSlot) {
        alert('Please select a preferred date and time slot.');
        return;
      }
    }

    if (step === 3) {
      if (!this.patientName.trim()) {
        alert('Please enter your full name.');
        return;
      }
      if (!this.patientEmail.trim() || !this.patientEmail.includes('@')) {
        alert('Please enter a valid email address.');
        return;
      }
      if (!this.patientPhone.trim() || this.patientPhone.length < 8) {
        alert('Please enter a valid mobile number.');
        return;
      }

      // Check if slot is mock
      if (this.selectedTimeSlot!.id.startsWith('mock_')) {
        this.lockedAppointmentId = 'mock_app_' + Math.floor(100000 + Math.random() * 900000);
        this.startLockTimer();
        this.currentStep = 3;
        return;
      }

      // Start lock and countdown
      this.isSubmitting = true;
      this.bookingService.bookAppointment(this.selectedTimeSlot!.id).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.success && res.data) {
            this.lockedAppointmentId = res.data.id;
            this.startLockTimer();
            this.currentStep = 3;
          } else {
            alert(res.message || 'Failed to lock slot. It may have been reserved already.');
          }
        },
        error: (err) => {
          this.isSubmitting = false;
          alert('This time slot is no longer available. Please select another slot.');
          console.error(err);
        }
      });
      return;
    }

    this.currentStep = step;
  }

  // Generates unique idempotency keys
  private generateIdempotencyKey(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  // Initialize unified Paymob checkouts
  payWithPaymob() {
    if (!this.lockedAppointmentId) {
      alert('No locked appointment found.');
      return;
    }

    if (this.lockedAppointmentId.startsWith('mock_')) {
      this.clearLockTimer();
      alert('Simulating Paymob Payment Success. Redirecting to verification results page...');
      this.router.navigate(['/payment/result'], {
        queryParams: { id: this.lockedAppointmentId, success: 'true' }
      });
      return;
    }

    this.isSubmitting = true;
    this.paymentErrorMessage = '';

    const idempotencyKey = this.generateIdempotencyKey();
    this.bookingService.createPayment(this.lockedAppointmentId, idempotencyKey, 'Online').subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success && res.data) {
          this.clearLockTimer();
          // Redirect browser to Paymob Unified Checkout portal
          const checkoutUrl = `https://accept.paymob.com/unifiedcheckout/?publicKey=${res.data.publicKey}&clientSecret=${res.data.clientSecret}`;
          window.location.href = checkoutUrl;
        } else {
          this.paymentErrorMessage = res.message || 'Failed to create payment intent.';
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.paymentErrorMessage = 'Payment Gateway Error: Paymob is unreachable. Please try again.';
        console.error(err);
      }
    });
  }
}
