import { Routes } from '@angular/router';
// Triggering Re-evaluation of Routes
import { About } from './about/about';
import { Contact } from './contact/contact';
import { Gallery } from './gallery/gallery';
import { Home } from './home/home';
import { Services } from './services/services';
import { Booking } from './booking/booking';
import { Login } from './auth/login/login';
import { Signup } from './auth/signup/signup';
import { Dashboard } from './dashboard/dashboard';
import { MedicalHistory } from './medical-history/medical-history';
import { VerifyEmail } from './auth/verify-email/verify-email';

export const routes: Routes = [
  { path: '' , redirectTo:"home" , pathMatch:'full' , title:"Home"},
  { path:"home",component:Home, title:"Home"},
  { path: 'services', component: Services, title: 'Services' },
  { path: 'about', component: About, title: 'About' },
  { path: 'gallery', component: Gallery, title: 'Gallery' },
  { path: 'contact', component: Contact , title: 'Contact', children:[
    { path: "home" , redirectTo:"home" , pathMatch:'full' , title:"Home"},
  ]},
  { path: 'book', component: Booking, title: 'Book Appointment' },
  { path: 'login', component: Login, title: 'Login' },
  { path: 'signup', component: Signup, title: 'Sign Up' },
  { path: 'dashboard', component: Dashboard, title: 'Dashboard' },
  { path: 'medical-history', component: MedicalHistory, title: 'Medical History' },
  { path: 'verify-email', component: VerifyEmail, title: 'Verify Email' }
];

