import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { Home } from './features/home/home';
import { Register } from './features/auth/register/register';
import { Login } from './features/auth/login/login';
import { Profile } from './features/profile/profile';

export const routes: Routes = [
    { path: 'auth/login', component: Login },
    { path: 'auth/register', component: Register },
    { path: '', component: Home },
    {
        path: 'profile',
        component: Profile,
        canActivate: [authGuard]
    },
];
