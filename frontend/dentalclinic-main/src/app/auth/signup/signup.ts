import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup {
  authService = inject(AuthService);
  router = inject(Router);

  firstName = '';
  middleName = '';
  lastName = '';
  emailAddress = '';
  phoneNumber = '';
  gender: 'Male' | 'Female' = 'Male';
  birthDate = '';
  passwordField = '';
  agreeTerms = false;
  showPasswordState = false;

  isSubmitting = false;
  errorMessage = '';

  toggleShowPassword(): void {
    this.showPasswordState = !this.showPasswordState;
  }

  // Helper validation for C# identity requirements
  validatePassword(pass: string): boolean {
    if (pass.length < 6) return false;
    const hasUpperCase = /[A-Z]/.test(pass);
    const hasLowerCase = /[a-z]/.test(pass);
    const hasNumber = /[0-9]/.test(pass);
    const hasSpecial = /[^A-Za-z0-9]/.test(pass);
    return hasUpperCase && hasLowerCase && hasNumber && hasSpecial;
  }

  onSubmit(): void {
    if (!this.firstName || !this.middleName || !this.lastName || !this.emailAddress || !this.phoneNumber || !this.birthDate || !this.passwordField) {
      this.errorMessage = 'Please fill all required fields';
      return;
    }
    if (!this.agreeTerms) {
      this.errorMessage = 'You must agree to the Terms of Service & Privacy Policy';
      return;
    }

    // Validate phone length and digits
    const cleanedPhone = this.phoneNumber.replace(/\s+/g, '');
    if (!/^\d+$/.test(cleanedPhone) || cleanedPhone.length > 11) {
      this.errorMessage = 'Phone number must be digits only and maximum 11 characters';
      return;
    }

    // Validate password complexity
    if (!this.validatePassword(this.passwordField)) {
      this.errorMessage = 'Password must be at least 6 characters, and contain at least 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const registerDto = {
      firstName: this.firstName,
      middleName: this.middleName,
      lastName: this.lastName,
      phone: cleanedPhone,
      gender: this.gender,
      birthDate: this.birthDate,
      email: this.emailAddress,
      password: this.passwordField
    };

    this.authService.register(registerDto).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          // Redirect to verify email and pass the returned userId
          // C# registration api returns data containing the userId (as GUID or string)
          const userId = response.data?.userId || response.data || '';
          alert('Registration successful! A verification code has been sent to your email.');
          this.router.navigate(['/verify-email'], { queryParams: { userId, email: this.emailAddress } });
        } else {
          this.errorMessage = this.mapErrorCode(response.errorCode, response.message);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Registration error:', err);
        this.errorMessage = 'Connection error: Unable to connect to the server. Please try again.';
      }
    });
  }

  private mapErrorCode(code: string | null, defaultMsg: string | null): string {
    if (!code) return defaultMsg || 'Registration failed.';
    switch (code) {
      case 'AUTH_EMAIL_ALREADY_EXISTS':
        return 'An account with this email address already exists.';
      case 'AUTH_INVALID_CREDENTIALS':
        return 'Invalid credentials provided.';
      default:
        return defaultMsg || 'An error occurred during registration. Please try again.';
    }
  }
}
