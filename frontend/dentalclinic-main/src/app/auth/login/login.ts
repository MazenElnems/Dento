import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  emailOrId: string = '';
  passwordField: string = '';
  rememberMe: boolean = false;
  showPasswordState: boolean = false;

  constructor(private router: Router) {}

  toggleShowPassword(): void {
    this.showPasswordState = !this.showPasswordState;
  }

  onSubmit(): void {
    if (!this.emailOrId || !this.passwordField) {
      alert('Please fill all fields');
      return;
    }
    // Simulate successful login
    alert('Login Successful!');
    this.router.navigate(['/home']);
  }
}
