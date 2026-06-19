import { Component, computed, inject, signal } from '@angular/core';
import { BookService } from '../../core/services/book.service';
import { Book, BookCategory, BookCategoryLabels, categorySlug } from '../../core/models/book.model';
import { Hero } from '../../shared/components/hero/hero';
import { UserBooks } from '../../shared/components/user-books/user-books';
import { BooksSlider } from '../../shared/components/books-slider/books-slider';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  imports: [Hero, UserBooks, BooksSlider],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected BookCategory = BookCategory;

  private bookService = inject(BookService);
  private authService = inject(AuthService);

  protected heroBooks = computed(() => {
    return this.booksTop().slice(0, 5);
  });
  protected booksUser = signal<Book[]>([]);
  protected booksTop = signal<Book[]>([]);
  protected books = signal<Book[]>([]);

  protected isLoggedIn = signal(this.authService.isLoggedIn());
  protected loadingUser = signal(true);
  protected loadingTop = signal(true);
  protected loadingCategories = signal(true);
  protected errorUser = signal(false);
  protected errorTop = signal(false);
  protected errorCategories = signal(false);

  protected categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({
      value,
      label: BookCategoryLabels[value],
      slug: 'category-' + categorySlug(value),
    }));

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.bookService.getMyBooks(1, 20).subscribe({
        next: (res) => {
          this.booksUser.set(res.items);
          this.loadingUser.set(false);
        },
        error: () => {
          this.loadingUser.set(false);
          this.errorUser.set(true);
        },
      });
    }
    this.bookService.getBooks(1, 10).subscribe({
      next: (res) => {
        this.booksTop.set(res.items);
        this.loadingTop.set(false);
      },
      error: () => {
        this.loadingTop.set(false);
        this.errorTop.set(true);
      },
    });
    this.bookService.getBooksFromEachCategory(1, 15).subscribe({
      next: (res) => {
        this.books.set(res.items);
        this.loadingCategories.set(false);
      },
      error: () => {
        this.loadingCategories.set(false);
        this.errorCategories.set(true);
      },
    });
  }
}
