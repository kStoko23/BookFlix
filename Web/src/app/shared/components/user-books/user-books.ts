import { Component, inject, signal } from '@angular/core';
import { Book } from '../../../core/models/book.model';
import { Router } from '@angular/router';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-books',
  imports: [],
  templateUrl: './user-books.html',
  styleUrl: './user-books.css',
})
export class UserBooks {
    private bookService = inject(BookService);
    private authService = inject(AuthService);
    private router = inject(Router);

    books = signal<Book[]>([]);
    currentIndex = signal<number>(0);
    private timer?: ReturnType<typeof setInterval>;

    ngOnInit() {
      if (this.authService.isAuthenticated()) {
        this.bookService.getBooks(1, 10).subscribe({
          next: res => {
            this.books.set(res.items);
          }
        });
      }
    }
}
