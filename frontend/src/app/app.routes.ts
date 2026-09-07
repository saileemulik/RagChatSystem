import { Routes } from '@angular/router';
import { Upload } from './components/upload/upload';
import { Chat } from './components/chat/chat';

export const routes: Routes = [
  { path: '', component: Upload, pathMatch: 'full' },
  { path: 'chat', component: Chat },
  { path: '**', redirectTo: '/' }
];
