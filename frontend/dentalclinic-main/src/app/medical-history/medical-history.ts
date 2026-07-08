import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { DataService, MedicalHistory as HistoryModel } from '../services/data.service';

@Component({
  selector: 'app-medical-history',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './medical-history.html',
  styleUrl: './medical-history.css'
})
export class MedicalHistory implements OnInit {
  authService = inject(AuthService);
  dataService = inject(DataService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  // Form Fields
  bloodPressure = 'Normal';
  diabetes = false;
  heartDisease = false;
  allergies = '';
  currentMedications = '';
  previousSurgeries = '';
  smokingStatus = 'Non-smoker';
  additionalNotes = '';

  isEditMode = false;
  isSubmitting = false;
  successMessage = '';
  redirectUrl = '';

  ngOnInit() {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    // Check if redirect query param exists (e.g. from booking page)
    this.route.queryParams.subscribe(params => {
      if (params['redirect']) {
        this.redirectUrl = params['redirect'];
      }
    });

    // Load existing history if present
    const existing = this.dataService.getMedicalHistory(user.id);
    if (existing) {
      this.isEditMode = true;
      this.bloodPressure = existing.bloodPressure;
      this.diabetes = existing.diabetes;
      this.heartDisease = existing.heartDisease;
      this.allergies = existing.allergies;
      this.currentMedications = existing.currentMedications;
      this.previousSurgeries = existing.previousSurgeries;
      this.smokingStatus = existing.smokingStatus;
      this.additionalNotes = existing.additionalNotes;
    }
  }

  onSubmit() {
    const user = this.authService.currentUser();
    if (!user) return;

    this.isSubmitting = true;
    
    const newHistory: HistoryModel = {
      userId: user.id,
      bloodPressure: this.bloodPressure,
      diabetes: this.diabetes,
      heartDisease: this.heartDisease,
      allergies: this.allergies,
      currentMedications: this.currentMedications,
      previousSurgeries: this.previousSurgeries,
      smokingStatus: this.smokingStatus,
      additionalNotes: this.additionalNotes,
      updatedAt: new Date().toLocaleString()
    };

    // Save history
    this.dataService.saveMedicalHistory(newHistory);

    setTimeout(() => {
      this.isSubmitting = false;
      this.successMessage = this.isEditMode 
        ? 'Medical history updated successfully!' 
        : 'Medical history submitted successfully!';
      
      setTimeout(() => {
        if (this.redirectUrl) {
          this.router.navigateByUrl(this.redirectUrl);
        } else {
          this.router.navigate(['/dashboard']);
        }
      }, 1500);
    }, 1000);
  }
}
