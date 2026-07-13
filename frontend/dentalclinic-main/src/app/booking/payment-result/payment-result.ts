import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { BookingService, AppointmentDetails } from '../../services/booking.service';
import { AuthService } from '../../services/auth.service';
import { DataService, Appointment as DataAppointment } from '../../services/data.service';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment-result.html',
  styleUrl: './payment-result.css'
})
export class PaymentResult implements OnInit, OnDestroy {
  route = inject(ActivatedRoute);
  router = inject(Router);
  bookingService = inject(BookingService);
  authService = inject(AuthService);
  dataService = inject(DataService);
  cdr = inject(ChangeDetectorRef);

  appointmentId = '';
  paymentSuccessQuery = false;
  
  appointmentDetails: AppointmentDetails | null = null;
  verificationStatus: 'verifying' | 'success' | 'failed' | 'timeout' = 'verifying';
  
  pollInterval: any = null;
  pollRetries = 0;
  maxPollRetries = 40; // Poll for up to 120 seconds (40 attempts * 3s)

  isCashBooking = false;
  hasResolved = false;

  ngOnInit() {
    console.log('PaymentResult: ngOnInit triggered');
    this.route.queryParams.subscribe(params => {
      console.log('PaymentResult: queryParams received:', params);
      if (this.hasResolved) {
        console.log('PaymentResult: already resolved, ignoring emission');
        return;
      }

      this.appointmentId = params['id'] || params['appointmentId'] || '';
      this.paymentSuccessQuery = params['success'] === 'true' || params['pending'] === 'false';
      this.isCashBooking = params['method'] === 'cash';

      if (!this.appointmentId) {
        this.appointmentId = params['merchant_order_id'] || '';
      }

      console.log('PaymentResult: appointmentId parsed:', this.appointmentId);

      if (this.appointmentId) {
        this.hasResolved = true;
        if (this.appointmentId.startsWith('mock_')) {
          console.log('PaymentResult: mock booking, resolving immediately to success');
          this.verificationStatus = 'success';
          this.appointmentDetails = {
            id: this.appointmentId,
            status: 'Confirmed',
            appointmentType: 'Consultation',
            createdAt: new Date().toISOString(),
            confirmedAt: new Date().toISOString(),
            canceledAt: null,
            slotId: 'mock_slot',
            slotDate: new Date().toISOString().split('T')[0],
            slotFrom: '10:00:00',
            slotTo: '10:45:00',
            slotStatus: 'Booked',
            slotLockedUntil: '',
            dentistId: 'mock_dentist',
            dentistName: this.appointmentId.includes('mazen') ? 'د. مازن النمس' : 'د. هنا بشرى',
            dentistSpecialty: this.appointmentId.includes('mazen') ? 'Cosmetic Dentistry' : 'Orthodontics',
            consultationFee: this.appointmentId.includes('mazen') ? 350 : 250,
            paymentId: this.isCashBooking ? 'pay_cash_mock' : 'pay_mock',
            paymentStatus: this.isCashBooking ? 'Pending' : 'Paid',
            paymentAmount: this.appointmentId.includes('mazen') ? 350 : 250,
            paymentCurrency: 'USD'
          };
          this.saveToLocalStorage(this.appointmentDetails);
        } else {
          console.log('PaymentResult: real booking, starting verification process');
          this.startStatusVerification();
        }
      } else {
        console.warn('PaymentResult: no appointmentId found, setting state to failed');
        this.verificationStatus = 'failed';
      }
    });
  }

  ngOnDestroy() {
    this.stopPolling();
  }

  startStatusVerification() {
    console.log('PaymentResult: startStatusVerification called');
    this.verificationStatus = 'verifying';
    this.pollRetries = 0;
    
    this.fetchAppointmentStatus();

    this.pollInterval = setInterval(() => {
      this.pollRetries++;
      console.log('PaymentResult: polling interval triggered, retry count:', this.pollRetries);
      if (this.pollRetries >= this.maxPollRetries) {
        console.warn('PaymentResult: polling timeout reached');
        this.stopPolling();
        this.verificationStatus = 'timeout';
        this.cdr.detectChanges();
      } else {
        this.fetchAppointmentStatus();
      }
    }, 3000);
  }

  fetchAppointmentStatus() {
    console.log('PaymentResult: fetchAppointmentStatus called, id =', this.appointmentId);
    if (this.appointmentId.startsWith('mock_')) {
      console.log('PaymentResult: detected mock appointment ID, registering 1.5s success timeout');
      setTimeout(() => {
        console.log('PaymentResult: mock success timeout triggered, stopping polling');
        this.stopPolling();
        this.appointmentDetails = {
          id: this.appointmentId,
          status: 'Confirmed',
          appointmentType: 'Consultation',
          createdAt: new Date().toISOString(),
          confirmedAt: new Date().toISOString(),
          canceledAt: null,
          slotId: 'mock_slot',
          slotDate: new Date().toISOString().split('T')[0],
          slotFrom: '10:00:00',
          slotTo: '10:45:00',
          slotStatus: 'Booked',
          slotLockedUntil: '',
          dentistId: 'mock_dentist',
          dentistName: this.appointmentId.includes('mazen') ? 'د. مازن النمس' : 'د. هنا بشرى',
          dentistSpecialty: this.appointmentId.includes('mazen') ? 'Cosmetic Dentistry' : 'Orthodontics',
          consultationFee: this.appointmentId.includes('mazen') ? 350 : 250,
          paymentId: 'pay_mock',
          paymentStatus: this.isCashBooking ? 'Pay at Clinic' : 'Paid',
          paymentAmount: this.appointmentId.includes('mazen') ? 350 : 250,
          paymentCurrency: 'USD'
        };
        console.log('PaymentResult: saving mock appointment to local storage');
        this.saveToLocalStorage(this.appointmentDetails);
        console.log('PaymentResult: setting verificationStatus to success');
        this.verificationStatus = 'success';
        this.cdr.detectChanges();
      }, 1500);
      return;
    }

    this.bookingService.getAppointment(this.appointmentId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.appointmentDetails = res.data;
          
          if (res.data.status === 'Confirmed' || res.data.paymentStatus === 'Paid' || (this.isCashBooking && res.data.paymentStatus === 'Pending')) {
            this.stopPolling();
            this.saveToLocalStorage(res.data);
            this.verificationStatus = 'success';
            this.cdr.detectChanges();
          } else if (res.data.status === 'Failed' || res.data.paymentStatus === 'Failed') {
            this.stopPolling();
            this.verificationStatus = 'failed';
            this.cdr.detectChanges();
          }
        }
      },
      error: (err) => {
        console.error('Error verifying appointment status:', err);
      }
    });
  }

  saveToLocalStorage(details: AppointmentDetails) {
    try {
      const user = this.authService.currentUser();
      if (!user) return;

      const list = this.dataService.getAppointments();
      if (list.some(a => a.id === details.id)) {
        return;
      }

      const app: DataAppointment = {
        id: details.id,
        patientId: user.id,
        patientName: user.name,
        patientPhone: user.phone || '',
        serviceId: 'consultation',
        serviceName: 'Consultation - ' + details.dentistSpecialty,
        price: details.consultationFee,
        dateStr: details.slotDate,
        time: details.slotFrom,
        dentistId: details.dentistId,
        dentistName: details.dentistName,
        status: 'upcoming'
      };

      this.dataService.saveAppointment(app);
    } catch (e) {
      console.error('Error saving appointment to localStorage:', e);
    }
  }

  stopPolling() {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
  }
}
