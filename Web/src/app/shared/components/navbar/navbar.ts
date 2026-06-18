import { Component, computed, effect, inject, signal, viewChild } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Button } from '../button/button';
import { Menu, MenuContent, MenuItem, MenuTrigger } from '@angular/aria/menu';
import { OverlayModule } from '@angular/cdk/overlay';
import { BookCategory, BookCategoryLabels } from '../../../core/models/book.model';

@Component({
  selector: 'app-navbar',
  imports: [
    RouterLink,
    RouterLinkActive,
    Button,
    Menu,
    MenuContent,
    MenuItem,
    MenuTrigger,
    OverlayModule,
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  authService = inject(AuthService);
  private router = inject(Router);
  menuOpen = signal(false);
  categoriesMenu = viewChild<Menu<BookCategory>>('categoriesMenu');
  mobileCategoriesOpen = signal(false);

  categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({ value, label: BookCategoryLabels[value] }));

  constructor() {
    effect(() => {
      document.body.style.overflow = this.menuOpen() ? 'hidden' : '';
    });
  }

  initials = computed(() => {
    const username = this.authService.user()?.username;
    return username ? username.slice(0, 2).toUpperCase() : 'U';
  });

  toggleMenu() {
    this.menuOpen.update((value) => !value);
  }
  closeMenu() {
    this.menuOpen.set(false);
    this.mobileCategoriesOpen.set(false);
  }

  logout() {
    this.closeMenu();
    this.authService.logout();
  }

  private categorySlug(category: BookCategory): string {
    return BookCategoryLabels[category].toLowerCase().replace(/\s+/g, '-');
  }

  goToCategory(category: BookCategory) {
    this.router.navigate(['/'], { fragment: 'category-' + this.categorySlug(category) });
    this.closeMenu();
  }

  toggleMobileCategories() {
    this.mobileCategoriesOpen.update((value) => !value);
  }
}
