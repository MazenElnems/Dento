import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './services.html',
  styleUrl: './services.css',
})
export class Services {
  services = [
    {
      id: 'whitening',
      icon: 'bi-brightness-high',
      name: 'Teeth Whitening',
      price: '$150',
      duration: '45 mins',
      desc: 'Enhance your teeth shade using our premium laser whitening treatment. Safe on enamel with instantaneous outcomes.',
      benefits: ['Up to 8 shades lighter', 'Enamel-safe diagnostic laser', 'Includes custom post-care kit'],
      image: 'https://images.unsplash.com/photo-1607613009820-a29f7bb81c04?w=500'
    },
    {
      id: 'implants',
      icon: 'bi-shield-check',
      name: 'Dental Implants',
      price: '$1,200',
      duration: '60 mins',
      desc: 'High-precision surgical implants designed to replace missing roots and support natural-looking porcelain crowns.',
      benefits: ['Lifetime warranty on implants', 'High-quality medical titanium', 'Full bite restoration'],
      image: 'https://images.unsplash.com/photo-1606811971618-4486d14f3f99?w=500'
    },
    {
      id: 'orthodontics',
      icon: 'bi-grid-3x3',
      name: 'Orthodontics',
      price: '$2,500',
      duration: '45 mins',
      desc: 'Align your smile with modern invisible aligners or high-durability ceramic braces customized for your facial profile.',
      benefits: ['Clear invisible aligners option', 'Digital 3D growth mapping', 'Comfortable corrective brackets'],
      image: 'https://images.unsplash.com/photo-1598256989800-fe5f95da9787?w=500'
    },
    {
      id: 'canal',
      icon: 'bi-bandaid',
      name: 'Root Canal Treatment',
      price: '$350',
      duration: '60 mins',
      desc: 'Save infected teeth and eliminate nerve pain with our modern micro-endodontic therapy. Comfortable and pain-free.',
      benefits: ['Highly localized anesthetics', 'Done in a single session', 'Prevents bone loss issues'],
      image: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=500'
    },
    {
      id: 'cosmetic',
      icon: 'bi-stars',
      name: 'Cosmetic Dentistry',
      price: '$800',
      duration: '60 mins',
      desc: 'Transform your aesthetics with Hollywood veneers, bonding, and smile contouring designed by cosmetic artisans.',
      benefits: ['Custom smile design mockup', 'Premium ultra-thin veneers', 'Perfect color matching'],
      image: 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=500'
    },
    {
      id: 'cleaning',
      icon: 'bi-heart-pulse',
      name: 'Teeth Cleaning & Hygiene',
      price: '$90',
      duration: '30 mins',
      desc: 'Deep plaque scaling, air-polishing, and fluoride application to maintain absolute oral hygiene and health.',
      benefits: ['Ultrasonic scaling device', 'Painless micro-air polish', 'Full clinical gum assessment'],
      image: 'https://images.unsplash.com/photo-1629909613654-28e377c37b09?w=500'
    }
  ];
}
