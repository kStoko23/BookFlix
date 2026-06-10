import { Component, computed, inject } from '@angular/core';
import { TokenService } from '../../../core/auth/token.service';
import { AuthService } from '../../../core/services/auth.service';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  authService = inject(AuthService);

  initials = computed(() => {
    const token = inject(TokenService).getToken();
    if (!token) return '';
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.username?.slice(0, 2).toUpperCase() ?? 'U';
  });
}
