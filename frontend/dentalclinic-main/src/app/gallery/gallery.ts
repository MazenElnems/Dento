import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface CaseItem {
  title: string;
  category: 'restoration' | 'orthodontics' | 'implants';
  description: string;
  details: string;
  beforeImg: string;
  afterImg: string;
}

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './gallery.html',
  styleUrl: './gallery.css',
})
export class Gallery {
  activeFilter: 'all' | 'restoration' | 'orthodontics' | 'implants' = 'all';

  cases: CaseItem[] = [
    {
      title: 'Full Arch Restoration',
      category: 'restoration',
      description: 'Rehabilitation of a damaged dental arch using porcelain crown structures.',
      details: 'Calibrated color shade matching with 3D intraoral digital diagnostics.',
      beforeImg: 'https://images.unsplash.com/photo-1598256989800-fe5f95da9787?w=500',
      afterImg: 'https://images.unsplash.com/photo-1607613009820-a29f7bb81c04?w=500'
    },
    {
      title: 'Bicuspid Arch Expansion',
      category: 'orthodontics',
      description: 'Orthodontic correction of crowded lateral teeth using customized aligners.',
      details: 'Achieved in 11 months with invisible clear thermoformed aligner grids.',
      beforeImg: 'https://images.unsplash.com/photo-1598256989800-fe5f95da9787?w=500',
      afterImg: 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=500'
    },
    {
      title: 'Titanium Implants Calibration',
      category: 'implants',
      description: 'Replacement of missing molar roots with biocompatible surgical titanium.',
      details: 'Full tooth functionality and bite force recovery restored in 12 weeks.',
      beforeImg: 'https://images.unsplash.com/photo-1606811971618-4486d14f3f99?w=500',
      afterImg: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=500'
    },
    {
      title: 'Cosmetic Veneer Contour',
      category: 'restoration',
      description: 'Hollywood smile makeover using ultra-thin custom ceramic veneers.',
      details: 'Pain-free prep and color-matching calibration for 8 upper anteriors.',
      beforeImg: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=500',
      afterImg: 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=500'
    },
    {
      title: 'Deep Overbite Realignment',
      category: 'orthodontics',
      description: 'Severe vertical overlap correction via clear aligner orthodontic pathways.',
      details: 'Precise micro-rotations verified using digital 3D dental diagnostics.',
      beforeImg: 'https://images.unsplash.com/photo-1598256989800-fe5f95da9787?w=500',
      afterImg: 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=500'
    },
    {
      title: 'Single Incisor Aesthetic Implant',
      category: 'implants',
      description: 'Front tooth replacement using a custom zirconia abutment and porcelain crown.',
      details: 'Seamless integration with neighboring teeth gum lines.',
      beforeImg: 'https://images.unsplash.com/photo-1606811971618-4486d14f3f99?w=500',
      afterImg: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=500'
    }
  ];

  get filteredCases() {
    if (this.activeFilter === 'all') {
      return this.cases;
    }
    return this.cases.filter(c => c.category === this.activeFilter);
  }

  setFilter(filter: 'all' | 'restoration' | 'orthodontics' | 'implants') {
    this.activeFilter = filter;
  }
}
