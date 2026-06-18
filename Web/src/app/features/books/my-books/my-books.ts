import { Component, inject, signal, computed, effect } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  startWith,
  map,
  catchError,
} from 'rxjs/operators';
import { of } from 'rxjs';
import { BookService } from '../../../core/services/book.service';
import { Book, BookCategory } from '../../../core/models/book.model';
import { BooksGrid } from '../../../shared/components/books-grid/books-grid';
import { BookFilters } from '../../../shared/components/book-filters/book-filters';
import { Button } from '../../../shared/components/button/button';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-my-books',
  imports: [BooksGrid, BookFilters, Button],
  templateUrl: './my-books.html',
})
export class MyBooks {
  private bookService = inject(BookService);

  search = signal('');
  category = signal<BookCategory | null>(null);
  page = signal(1);

  constructor() {
    //if filters change, reset requested page to 1
    effect(() => {
      this.search();
      this.category();
      this.page.set(1);
    });
  }

  private filters$ = toObservable(
    // observe filters
    computed(() => ({ search: this.search(), category: this.category(), page: this.page() })),
  );

  // result as signal, with debounce, waits for filter observable changes before making new request
  private result = toSignal(
    this.filters$.pipe(
      debounceTime(300),
      distinctUntilChanged(
        (a, b) => a.search === b.search && a.category === b.category && a.page === b.page,
      ),
      switchMap(({ search, category, page }) =>
        this.bookService.getBooks(page, PAGE_SIZE, search, category ?? undefined).pipe(
          map((res) => ({
            books: res.items,
            totalCount: res.totalCount,
            loading: false,
            error: false,
          })),
          startWith({ books: [] as Book[], totalCount: 0, loading: true, error: false }),
          catchError(() => of({ books: [] as Book[], totalCount: 0, loading: false, error: true })),
        ),
      ),
    ),
    { initialValue: { books: [] as Book[], totalCount: 0, loading: true, error: false } },
  );

  // exposed result properties as signals
  books = computed(() => this.result().books);
  loading = computed(() => this.result().loading);
  error = computed(() => this.result().error);
  totalCount = computed(() => this.result().totalCount);

  // pagination calc logic
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  hasPrev = computed(() => this.page() > 1);
  hasNext = computed(() => this.page() < this.totalPages());

  prevPage() {
    if (this.hasPrev()) this.page.update((p) => p - 1);
  }
  nextPage() {
    if (this.hasNext()) this.page.update((p) => p + 1);
  }
}
