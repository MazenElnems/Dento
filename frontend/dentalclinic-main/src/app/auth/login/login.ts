import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  authService = inject(AuthService);
  router = inject(Router);

  emailOrId = '';
  passwordField = '';
  rememberMe = false;
  showPasswordState = false;

  isSubmitting = false;
  errorMessage = '';

  toggleShowPassword(): void {
    this.showPasswordState = !this.showPasswordState;
  }

  onSubmit(): void {
    if (!this.emailOrId || !this.passwordField) {
      this.errorMessage = 'Please enter your email and password.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.login(this.emailOrId, this.passwordField).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          const user = this.authService.currentUser();
          if (user) {
            if (user.role === 'patient') {
              this.router.navigate(['/dashboard']);
            } else {
              // Non-patients should not be allowed into the main app client portal
              alert('Staff members should log in using the admin portal. You will be redirected.');
              this.authService.logout();
              this.router.navigate(['/login']);
            }
          }
        } else {
          this.errorMessage = this.mapErrorCode(response.errorCode, response.message);
          
          // If email is not verified, navigate to verification screen
          if (response.errorCode === 'AUTH_EMAIL_NOT_VERIFIED') {
            const userId = response.data?.userId || response.data || '';
            setTimeout(() => {
              this.router.navigate(['/verify-email'], { queryParams: { userId, email: this.emailOrId } });
            }, 2000);
          }
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Login error:', err);
        this.errorMessage = 'Connection error: Unable to connect to the server. Please try again.';
      }
    });
  }

  private mapErrorCode(code: string | null, defaultMsg: string | null): string {
    if (!code) return defaultMsg || 'Login failed.';
    switch (code) {
      case 'AUTH_INVALID_CREDENTIALS':
        return 'The email or password you entered is incorrect.';
      case 'AUTH_EMAIL_NOT_VERIFIED':
        return 'Your email address is not verified yet. Redirecting you to verification page...';
      case 'AUTH_USER_NOT_FOUND':
        return 'No account was found with this email address.';
      default:
        return defaultMsg || 'Login failed. Please try again.';
    }
  }
}
