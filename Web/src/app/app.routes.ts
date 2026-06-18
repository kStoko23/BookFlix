import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { Home } from './features/home/home';
import { Register } from './features/auth/register/register';
import { Login } from './features/auth/login/login';
import { Profile } from './features/profile/profile';
import { BookDetails } from './features/books/book-details/book-details';
import { MyBooksPage } from './features/books/my-books-page/my-books-page';

export const routes: Routes = [
  { path: 'auth/login', component: Login },
  { path: 'auth/register', component: Register },
  { path: 'books/:id', component: BookDetails },
  { path: 'my-list', component: MyBooksPage, canActivate: [authGuard] },
  { path: '', component: Home },
  { path: 'profile', component: Profile, canActivate: [authGuard] },
];
