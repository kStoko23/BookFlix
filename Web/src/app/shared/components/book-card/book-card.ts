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
    return `https://picsum.photos/seed/book${book.id}/250/450.webp`;
  }

  navigateToBook(id: number) {
    this.router.navigate(['/books', id]);
  }
}
