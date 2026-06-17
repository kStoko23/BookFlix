import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Button } from '../button/button';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, Button],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  authService = inject(AuthService);

  initials = computed(() => {
    const username = this.authService.user()?.username;
    return username ? username.slice(0, 2).toUpperCase() : 'U';
  });
}
