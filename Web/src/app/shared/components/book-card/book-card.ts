import { Component, inject, input, InputSignal } from '@angular/core';
import { Book } from '../../../core/models/book.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-book-card',
  imports: [],
  templateUrl: './book-card.html',
  styleUrl: './book-card.css',
})
export class BookCard {
  book = input.required<Book>();
  router = inject(Router);

  getImageUrl(book: Book): string {
  const colors = ['1a3a5c', '7c1f2e', '2d5a27', '3d3060', '5c3a1a', '1a4a4a', '4a1a1a'];
  const color = colors[book.id % colors.length];
  const title = book.title.length > 20 ? book.title.slice(0, 18) + '…' : book.title;
  const author = book.author.split(' ').slice(0, 2).join(' ');

  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" width="250" height="400" viewBox="0 0 250 400">
      <rect width="250" height="400" rx="3" fill="#${color}"/>
      <rect x="0" y="0" width="8" height="400" fill="rgba(0,0,0,0.2)"/>
      <rect x="15" y="15" width="220" height="370" rx="2" fill="none" stroke="rgba(255,255,255,0.1)" stroke-width="0.5"/>
      <line x1="20" y1="280" x2="230" y2="280" stroke="rgba(255,255,255,0.2)" stroke-width="0.5"/>
      <text x="125" y="240" text-anchor="middle" font-family="Georgia,serif" font-size="16" font-weight="700" fill="rgba(255,255,255,0.95)">${title}</text>
      <text x="125" y="310" text-anchor="middle" font-family="Georgia,serif" font-size="12" fill="rgba(255,255,255,0.6)">${author}</text>
    </svg>`;

  return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
}

  navigateToBook(id: number) {
    this.router.navigate(['/books', id]);
  }
}
