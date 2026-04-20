import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.html',
  styleUrls: ['./footer.css']
})
export class FooterComponent {
  currentYear = new Date().getFullYear();

  services = [
    'Teeth Whitening',
    'Dental Implants',
    'Orthodontics',
    'Root Canal',
    'Cosmetic Dentistry',
    'Teeth Cleaning',
  ];

  quickLinks = ['Home', 'Services', 'About', 'Gallery', 'Contact'];

  socials = [
    { name: 'Facebook',   icon: 'bi-facebook' },
    { name: 'Instagram',  icon: 'bi-instagram' },
    { name: 'Twitter',    icon: 'bi-twitter-x' },
    { name: 'WhatsApp',   icon: 'bi-whatsapp' },
  ];
}