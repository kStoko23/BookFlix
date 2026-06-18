import { Component, inject } from '@angular/core';
import { BookService } from '../../../core/services/book.service';
import { BooksGrid } from '../../../shared/components/books-grid/books-grid';
import { BookFilters } from '../../../shared/components/book-filters/book-filters';
import { Button } from '../../../shared/components/button/button';
import { createBooksListState } from '../book-list.state';

@Component({
  selector: 'app-my-books-page',
  imports: [BooksGrid, BookFilters, Button],
  templateUrl: './my-books-page.html',
})
export class MyBooksPage {
  private bookService = inject(BookService);
  state = createBooksListState(this.bookService.getMyBooks.bind(this.bookService));
}
