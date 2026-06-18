import { signal, computed, effect } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  map,
  startWith,
  catchError,
} from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { Book, BookCategory, PagedResponse } from '../../core/models/book.model';

const PAGE_SIZE = 20;

//input fetching method
type BooksFetcher = (
  page: number,
  pageSize: number,
  search: string,
  category?: BookCategory,
) => Observable<PagedResponse<Book>>;

export function createBooksListState(fetcher: BooksFetcher) {
  const search = signal('');
  const category = signal<BookCategory | null>(null);
  const page = signal(1);

  //reset page when filters change
  effect(() => {
    search();
    category();
    page.set(1);
  });

  //observe filters for changes
  const filters$ = toObservable(
    computed(() => ({ search: search(), category: category(), page: page() })),
  );

  const result = toSignal(
    filters$.pipe(
      debounceTime(300),
      distinctUntilChanged(
        (a, b) => a.search === b.search && a.category === b.category && a.page === b.page,
      ),
      switchMap(({ search, category, page }) =>
        fetcher(page, PAGE_SIZE, search, category ?? undefined).pipe(
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

  const totalCount = computed(() => result().totalCount);
  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount() / PAGE_SIZE)));

  //return state as signals
  return {
    search,
    category,
    page,
    books: computed(() => result().books),
    loading: computed(() => result().loading),
    error: computed(() => result().error),
    totalCount,
    totalPages,
    hasPrev: computed(() => page() > 1),
    hasNext: computed(() => page() < totalPages()),
    prevPage: () => {
      if (page() > 1) page.update((p) => p - 1);
    },
    nextPage: () => {
      if (page() < totalPages()) page.update((p) => p + 1);
    },
  };
}
