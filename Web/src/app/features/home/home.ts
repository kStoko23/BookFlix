import { Component, inject, signal } from '@angular/core';
import { BookService } from '../../core/services/book.service';
import { Book, PagedResponse } from '../../core/models/book.model';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private bookService = inject(BookService);

  books = signal<Book[]>([]);
  totalCount = signal<number>(0);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit() {
    this.bookService.getBooks().subscribe({
      next: (response: PagedResponse<Book>) => {
        this.books.set(response.items);
        this.totalCount.set(response.totalCount);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.message + 'Nie udało się załadować książek.');
        this.isLoading.set(false);
      }
    });
  }
}
