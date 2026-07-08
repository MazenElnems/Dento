import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.css'
})
export class VerifyEmail implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  userId = '';
  email = '';
  code = '';
  
  isSubmitting = false;
  isResending = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.userId = params['userId'] || '';
      this.email = params['email'] || '';

      if (!this.userId) {
        this.errorMessage = 'Invalid verification link. Please check your email or sign up again.';
      }
    });
  }

  onSubmit() {
    if (!this.code.trim()) {
      this.errorMessage = 'Please enter the verification code.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.verifyEmail(this.code.trim(), this.userId).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.successMessage = 'Your email has been verified successfully! Redirecting you to login...';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2500);
        } else {
          this.errorMessage = this.mapErrorCode(response.errorCode, response.message);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Verification error:', err);
        this.errorMessage = 'Connection error: Unable to verify code. Please try again.';
      }
    });
  }

  resendCode() {
    if (!this.email) return;

    this.isResending = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.sendVerificationCode(this.email).subscribe({
      next: (response) => {
        this.isResending = false;
        if (response.success) {
          this.successMessage = 'A new verification code has been sent to your email.';
        } else {
          this.errorMessage = response.message || 'Failed to resend verification code.';
        }
      },
      error: (err) => {
        this.isResending = false;
        console.error('Resend error:', err);
        this.errorMessage = 'Connection error: Unable to resend verification code.';
      }
    });
  }

  private mapErrorCode(code: string | null, defaultMsg: string | null): string {
    if (!code) return defaultMsg || 'Verification failed.';
    switch (code) {
      case 'AUTH_INVALID_VERIFICATION_CODE':
        return 'The verification code entered is incorrect or has expired. Please try again.';
      default:
        return defaultMsg || 'Verification failed. Please check the code and try again.';
    }
  }
}
