import { Component, inject, signal } from '@angular/core';
import { BookService } from '../../core/services/book.service';
import { Book, PagedResponse } from '../../core/models/book.model';
import { Hero } from '../../shared/components/hero/hero';
import { UserBooks } from "../../shared/components/user-books/user-books";

@Component({
  selector: 'app-home',
  imports: [Hero, UserBooks],
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

  }
}
