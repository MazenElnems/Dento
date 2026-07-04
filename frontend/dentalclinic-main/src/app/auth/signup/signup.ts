import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup {
  fullName: string = '';
  emailAddress: string = '';
  phoneNumber: string = '';
  passwordField: string = '';
  agreeTerms: boolean = false;
  showPasswordState: boolean = false;

  constructor(private router: Router) {}

  toggleShowPassword(): void {
    this.showPasswordState = !this.showPasswordState;
  }

  onSubmit(): void {
    if (!this.fullName || !this.emailAddress || !this.phoneNumber || !this.passwordField) {
      alert('Please fill all required fields');
      return;
    }
    if (!this.agreeTerms) {
      alert('You must agree to the Terms of Service & Privacy Policy');
      return;
    }
    // Simulate successful registration
    alert('Account Created Successfully!');
    this.router.navigate(['/home']);
  }
}
