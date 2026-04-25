import { Routes } from '@angular/router';
import { About } from './about/about';
import { Contact } from './contact/contact';
import { Gallery } from './gallery/gallery';
import { Home } from './home/home';
import { Services } from './services/services';

export const routes: Routes = [
  { path: '' , redirectTo:"home" , pathMatch:'full' , title:"Home"},
  { path:"home",component:Home, title:"Home"},
  { path: 'services', component: Services },
  { path: 'about', component: About },
  { path: 'gallery', component: Gallery },
  { path: 'contact', component: Contact ,children:[
    { path: "home" , redirectTo:"home" , pathMatch:'full' , title:"Home"},
  ]},
];
