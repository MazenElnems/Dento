import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface Doctor {
  name: string;
  role: string;
  desc: string;
  image: string;
}

interface FAQ {
  question: string;
  answer: string;
}

interface Article {
  tag: string;
  date: string;
  title: string;
  desc: string;
  image: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  clinicImageUrl = '../../../assets/images/dental clinic.jpg';
  activeFaqIndex: number | null = null;

  doctors: Doctor[] = [
    {
      name: 'Dr. Hana Boshra',
      role: 'CHIEF RESTORATIVE DENTIST',
      desc: 'Specializing in biomimetic ceramic integration and full-mouth rehabilitation.',
      image: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400'
    },
    {
      name: 'Dr. Layla Mansour',
      role: 'LEAD ORTHODONTIST',
      desc: 'Expert in micro-layered composite veneers and non-invasive dental alignment.',
      image: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400'
    },
    {
      name: 'Dr. Mazen Elnems',
      role: 'IMPLANTOLOGY SPECIALIST',
      desc: 'Pioneer in 3D cone-beam analysis and computer-guided implant placement.',
      image: 'https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400'
    },
    {
      name: 'Dr. Tareq El-Masry',
      role: 'MICRO-ENDODONTICS LEAD',
      desc: 'Expert in microscopic canal therapies and regenerative dental medicine.',
      image: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400'
    }
  ];

  faqs: FAQ[] = [
    {
      question: 'Do you accept international medical insurance?',
      answer: 'Yes, we collaborate directly with leading global insurance providers to ensure seamless, pre-authorized coverage for international patients.'
    },
    {
      question: 'Is sedation dentistry available for complex cases?',
      answer: 'Absolutely. We offer customized IV and oral conscious sedation options administered by certified anesthesiologists for anxiety-free procedures.'
    },
    {
      question: 'What is the expected lifespan of porcelain restorations?',
      answer: 'With proper clinical maintenance, our high-grade eMax and Zirconia restorations are engineered to last between 15 to 25 years.'
    },
    {
      question: 'How soon can I schedule my first digital scan?',
      answer: 'You can typically secure a 3D intraoral digital diagnostic scan within 24 to 48 hours via our live slot scheduler.'
    }
  ];

  articles: Article[] = [
    {
      tag: 'TRENDS',
      date: 'MAR 24, 2026',
      title: 'The Rise of Biomimetic Restorative Architecture',
      desc: 'How bringing nature-back dental design design leads to longer-lasting dental crowns and better aesthetics.',
      image: 'https://images.unsplash.com/photo-1606811971618-4486d14f3f99?w=500'
    },
    {
      tag: 'DIAGNOSTICS',
      date: 'MAR 15, 2026',
      title: 'Microscopic Precision in Modern Endodontics',
      desc: 'Why magnification at the level of the fiber optic is saving natural teeth and ensuring procedure success.',
      image: 'https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=500'
    },
    {
      tag: 'TECHNOLOGY',
      date: 'FEB 28, 2026',
      title: 'AI-Driven Treatment Planning: The Future is Here',
      desc: 'Presenting dental lab networks and tracing structural design with unprecedented outcomes.',
      image: 'https://images.unsplash.com/photo-1629909613654-28e377c37b09?w=500'
    }
  ];

  toggleFaq(index: number): void {
    this.activeFaqIndex = this.activeFaqIndex === index ? null : index;
  }

  onSynchronize(): void {}
  onPortfolio(): void {}
}
