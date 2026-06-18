import { Component, inject } from '@angular/core';
import { BookService } from '../../../core/services/book.service';
import { Button } from '../../../shared/components/button/button';
import { BookFilters } from '../../../shared/components/book-filters/book-filters';
import { BooksGrid } from '../../../shared/components/books-grid/books-grid';
import { createBooksListState } from '../book-list.state';

@Component({
  selector: 'app-all-books-page',
  imports: [BooksGrid, BookFilters, Button],
  templateUrl: './all-books-page.html',
  styleUrl: './all-books-page.css',
})
export class AllBooksPage {
  private bookService = inject(BookService);
  state = createBooksListState(this.bookService.getBooks.bind(this.bookService));
}
