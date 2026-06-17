import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Footer } from './shared/components/footer/footer';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Web');

  private router = inject(Router);

  showNavbar(): boolean {
    return !this.router.url.startsWith('/auth');
  }
  showFooter(): boolean {
    return !this.router.url.startsWith('/auth');
  }
}
