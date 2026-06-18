import { Component, inject, signal } from '@angular/core';
import { BookService } from '../../core/services/book.service';
import {
  Book,
  BookCategory,
  BookCategoryLabels,
  categorySlug,
  PagedResponse,
} from '../../core/models/book.model';
import { Hero } from '../../shared/components/hero/hero';
import { UserBooks } from '../../shared/components/user-books/user-books';
import { BooksSlider } from '../../shared/components/books-slider/books-slider';

@Component({
  selector: 'app-home',
  imports: [Hero, UserBooks, BooksSlider],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected BookCategory = BookCategory;
  private bookService = inject(BookService);
  protected books = signal<Book[]>([]);
  protected booksTop = signal<Book[]>([]);

  protected categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({
      value,
      label: BookCategoryLabels[value],
      slug: 'category-' + categorySlug(value),
    }));

  ngOnInit() {
    this.bookService.getBooks(1, 10).subscribe((response: PagedResponse<Book>) => {
      this.booksTop.set(response.items);
    });
    this.bookService.getBooksFromEachCategory(1, 15).subscribe((response: PagedResponse<Book>) => {
      this.books.set(response.items);
    });
  }
}
