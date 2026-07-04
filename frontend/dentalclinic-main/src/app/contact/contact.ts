import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {
  // Form models
  patientName = '';
  patientEmail = '';
  patientPhone = '';
  inquiryMessage = '';

  isSubmitting = false;
  showSuccessModal = false;
  errorMessage = '';

  submitInquiry() {
    // Basic field validation
    if (!this.patientName.trim()) {
      this.errorMessage = 'Please enter your full name.';
      return;
    }
    if (!this.patientEmail.trim() || !this.patientEmail.includes('@')) {
      this.errorMessage = 'Please enter a valid email address.';
      return;
    }
    if (!this.inquiryMessage.trim()) {
      this.errorMessage = 'Please enter your inquiry details.';
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;

    // Simulate server request delay
    setTimeout(() => {
      this.isSubmitting = false;
      this.showSuccessModal = true;
      this.resetForm();
    }, 1500);
  }

  resetForm() {
    this.patientName = '';
    this.patientEmail = '';
    this.patientPhone = '';
    this.inquiryMessage = '';
  }

  closeModal() {
    this.showSuccessModal = false;
  }
}
