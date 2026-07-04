import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface Service {
  id: string;
  name: string;
  price: number;
  duration: string;
}

interface TimeSlot {
  time: string;
  available: boolean;
}

interface DaySlots {
  date: Date;
  dateStr: string;
  dayName: string;
  slots: TimeSlot[];
}

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './booking.html',
  styleUrl: './booking.css'
})
export class Booking implements OnInit {
  currentStep = 1;

  // Services available for booking
  services: Service[] = [
    { id: 'whitening', name: 'Teeth Whitening', price: 150, duration: '45 mins' },
    { id: 'implants', name: 'Dental Implants', price: 1200, duration: '60 mins' },
    { id: 'ortho', name: 'Orthodontics (Braces)', price: 2500, duration: '45 mins' },
    { id: 'canal', name: 'Root Canal Treatment', price: 350, duration: '60 mins' },
    { id: 'cosmetic', name: 'Cosmetic Dentistry', price: 800, duration: '60 mins' },
    { id: 'cleaning', name: 'Teeth Cleaning & Hygiene', price: 90, duration: '30 mins' }
  ];

  selectedService: Service | null = null;
  selectedDay: DaySlots | null = null;
  selectedTimeSlot: string | null = null;

  // Form Fields
  patientName = '';
  patientEmail = '';
  patientPhone = '';

  // Payment Form Fields
  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';
  cardName = '';

  isSubmitting = false;
  paymentErrorMessage = '';
  assignedDentist = '';
  confirmationId = '';

  // Grid of available days and slots for the next 4 weeks
  calendarDays: DaySlots[] = [];

  dentists = [
    'Dr. Hana Boshra (Lead Orthodontist)',
    'Dr. Mazen Elnems (Implant Specialist)'
  ];

  ngOnInit() {
    this.generateCalendarSlots();
    // Default select first service
    this.selectedService = this.services[0];
  }

  generateCalendarSlots() {
    const days: DaySlots[] = [];
    const today = new Date();
    
    // Generate next 28 days (4 weeks)
    for (let i = 1; i <= 28; i++) {
      const nextDate = new Date(today);
      nextDate.setDate(today.getDate() + i);
      
      // Skip Fridays (clinic holiday)
      if (nextDate.getDay() === 5) {
        continue;
      }

      const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      const dayName = dayNames[nextDate.getDay()];
      const dateStr = nextDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });

      // Generate standard slots from 9:00 AM to 5:00 PM
      const slots: TimeSlot[] = [
        { time: '09:00 AM', available: Math.random() > 0.15 },
        { time: '09:45 AM', available: Math.random() > 0.2 },
        { time: '10:30 AM', available: Math.random() > 0.1 },
        { time: '11:15 AM', available: Math.random() > 0.25 },
        { time: '01:00 PM', available: Math.random() > 0.15 },
        { time: '01:45 PM', available: Math.random() > 0.3 },
        { time: '02:30 PM', available: Math.random() > 0.05 },
        { time: '03:15 PM', available: Math.random() > 0.2 },
        { time: '04:00 PM', available: Math.random() > 0.4 }
      ];

      days.push({
        date: nextDate,
        dateStr,
        dayName,
        slots
      });
    }
    this.calendarDays = days;
    // Default select first available day
    if (this.calendarDays.length > 0) {
      this.selectedDay = this.calendarDays[0];
    }
  }

  selectService(service: Service) {
    this.selectedService = service;
  }

  selectDay(day: DaySlots) {
    this.selectedDay = day;
    this.selectedTimeSlot = null; // Reset slot
  }

  selectTime(slot: TimeSlot) {
    if (slot.available) {
      this.selectedTimeSlot = slot.time;
    }
  }

  goToStep(step: number) {
    if (step === 2) {
      // Validation for Step 1
      if (!this.selectedService) {
        alert('Please select a service.');
        return;
      }
      if (!this.selectedDay || !this.selectedTimeSlot) {
        alert('Please select a preferred date and time slot.');
        return;
      }
    }

    if (step === 3) {
      // Validation for Step 2
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
    }

    this.currentStep = step;
  }

  submitPayment(simulateSuccess: boolean) {
    // Basic validation
    if (!this.cardName.trim()) {
      this.paymentErrorMessage = 'Please enter Cardholder Name.';
      return;
    }
    if (this.cardNumber.replace(/\s+/g, '').length < 16) {
      this.paymentErrorMessage = 'Please enter a valid 16-digit Card Number.';
      return;
    }
    if (!this.cardExpiry.includes('/')) {
      this.paymentErrorMessage = 'Please enter expiration date (MM/YY).';
      return;
    }
    if (this.cardCvv.length < 3) {
      this.paymentErrorMessage = 'Please enter a valid CVV.';
      return;
    }

    this.isSubmitting = true;
    this.paymentErrorMessage = '';

    setTimeout(() => {
      this.isSubmitting = false;
      if (simulateSuccess) {
        // Auto-assign Dentist (FR-7)
        const randomIndex = Math.floor(Math.random() * this.dentists.length);
        this.assignedDentist = this.dentists[randomIndex];
        
        // Generate mock confirmation ID
        this.confirmationId = 'DC-' + Math.floor(100000 + Math.random() * 900000);
        this.currentStep = 4;
      } else {
        this.paymentErrorMessage = 'Transaction Declined: Insufficient funds or invalid details. Please try again.';
      }
    }, 1500);
  }
}
