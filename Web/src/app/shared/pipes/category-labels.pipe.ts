import { Pipe, PipeTransform } from '@angular/core';
import { BookCategory, BookCategoryLabels } from '../../core/models/book.model';

@Pipe({
  name: 'categoryLabels',
})
export class CategoryLabelsPipe implements PipeTransform {
  transform(value: BookCategory | number): string {
    if (value == null) return '';
    return BookCategoryLabels[value as BookCategory] ?? 'Unknown';
  }
}
