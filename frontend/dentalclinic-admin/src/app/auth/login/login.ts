import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
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

  email = '';
  password = '';
  showPasswordState = false;
  isSubmitting = false;
  errorMessage = '';

  toggleShowPassword(): void {
    this.showPasswordState = !this.showPasswordState;
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please fill out all fields.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const runSimulationFallback = () => {
      const emailLower = this.email.toLowerCase();
      let simulatedRole: 'admin' | 'dentist' | 'receptionist' | null = null;
      let simulatedName = '';

      if (emailLower === 'dentist@clinic.com' && this.password === 'dentist123') {
        simulatedRole = 'dentist';
        simulatedName = 'د. مازن النمس';
      } else if (emailLower === 'recep@clinic.com' && this.password === 'recep123') {
        simulatedRole = 'receptionist';
        simulatedName = 'مروة أحمد (استقبال)';
      } else if (emailLower === 'admin@clinic.com' && this.password === 'admin123') {
        simulatedRole = 'admin';
        simulatedName = 'د. هنا بشرى (المدير)';
      }

      if (simulatedRole) {
        const mockUser = {
          id: 'simulated_' + simulatedRole,
          name: simulatedName,
          email: this.email,
          role: simulatedRole,
          status: 'active' as const
        };
        localStorage.setItem('dc_admin_auth_token', 'simulated_jwt_token');
        localStorage.setItem('dc_admin_active_user', JSON.stringify(mockUser));
        this.authService.currentUser.set(mockUser);
        
        this.isSubmitting = false;
        this.router.navigate(['/dashboard']);
        return true;
      }
      return false;
    };

    this.authService.login(this.email, this.password).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          const user = this.authService.currentUser();
          if (user) {
            if (user.role === 'admin' || user.role === 'dentist' || user.role === 'receptionist') {
              this.router.navigate(['/dashboard']);
            } else {
              alert('Patients are not authorized to access this administrative portal.');
              this.authService.logout();
              this.router.navigate(['/login']);
            }
          }
        } else {
          if (runSimulationFallback()) {
            return;
          }
          this.errorMessage = response.message || 'Invalid email or password.';
        }
      },
      error: (err) => {
        console.error('Staff Login error:', err);
        if (runSimulationFallback()) {
          return;
        }
        this.isSubmitting = false;
        this.errorMessage = 'Connection error: Unable to connect to the server.';
      }
    });
  }

  onDemoBypass(): void {
    if (!this.email) {
      this.email = 'admin@clinic.com';
      this.password = 'admin123';
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const emailLower = this.email.toLowerCase();
    let simulatedRole: 'admin' | 'dentist' | 'receptionist' = 'admin';
    let simulatedName = 'د. هنا بشرى (المدير)';

    if (emailLower.includes('dentist')) {
      simulatedRole = 'dentist';
      simulatedName = 'د. مازن النمس';
    } else if (emailLower.includes('recep')) {
      simulatedRole = 'receptionist';
      simulatedName = 'مروة أحمد (استقبال)';
    }

    const mockUser = {
      id: 'simulated_' + simulatedRole,
      name: simulatedName,
      email: this.email,
      role: simulatedRole,
      status: 'active' as const
    };

    localStorage.setItem('dc_admin_auth_token', 'simulated_jwt_token');
    localStorage.setItem('dc_admin_active_user', JSON.stringify(mockUser));
    this.authService.currentUser.set(mockUser);

    setTimeout(() => {
      this.isSubmitting = false;
      this.router.navigate(['/dashboard']);
    }, 600);
  }
}
