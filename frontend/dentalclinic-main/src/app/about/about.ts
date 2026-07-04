import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './about.html',
  styleUrl: './about.css',
})
export class About {
  team = [
    {
      name: 'Dr. Hana Boshra',
      role: 'Lead Orthodontist',
      bio: 'Dr. Hana is an expert in clear aligner solutions and reconstructive braces, graduating from Cairo University with over 10 years of clinical experience.',
      qualifications: ['B.D.S. Dental Surgery', 'M.Sc. Orthodontics', 'Certified Invisalign Provider']
    },
    {
      name: 'Dr. Mazen Elnems',
      role: 'Implant & Restoration Specialist',
      bio: 'Dr. Mazen specializes in surgical implant placement and full-mouth smile restorations, employing advanced digital intraoral design scans.',
      qualifications: ['B.D.S. Dental Surgery', 'D.D.S. Implantology', 'Member of ICOI']
    }
  ];

  technologies = [
    {
      name: '3D Intraoral Scanner',
      desc: 'Creates a high-fidelity digital map of your mouth, replacing uncomfortable physical plaster impressions.'
    },
    {
      name: 'Soft-Tissue Dental Lasers',
      desc: 'Provides pain-free, sterile, and rapid healing therapies for gum contouring and dental cleanings.'
    },
    {
      name: 'Low-Radiation Digital X-Ray',
      desc: 'Instant high-resolution diagnostic imaging that reduces radiation exposure by up to 90%.'
    }
  ];
}
