import { Component, input, output } from '@angular/core';
import { BookCategory, BookCategoryLabels } from '../../../core/models/book.model';

@Component({
  selector: 'app-book-filters',
  imports: [],
  templateUrl: './book-filters.html',
})
export class BookFilters {
  search = input<string>('');
  category = input<BookCategory | null>(null);

  searchChange = output<string>();
  categoryChange = output<BookCategory | null>();

  categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({ value, label: BookCategoryLabels[value] }));

  onCategoryChange(raw: string) {
    //if its null then get all categories, if number is given convert to BookCategory
    this.categoryChange.emit(raw === '' ? null : (Number(raw) as BookCategory));
  }

  clear() {
    this.searchChange.emit('');
    this.categoryChange.emit(null);
  }
}
