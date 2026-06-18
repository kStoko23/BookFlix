import { Component, input } from '@angular/core';
import { Book } from '../../../core/models/book.model';
import { BookCard } from '../book-card/book-card';

@Component({
  selector: 'app-books-grid',
  imports: [BookCard],
  templateUrl: './books-grid.html',
  styleUrl: './books-grid.css',
})
export class BooksGrid {
  books = input<Book[]>([]);
}
