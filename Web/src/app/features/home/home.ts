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

  categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({
      value,
      label: BookCategoryLabels[value],
      slug: 'category-' + categorySlug(value),
    }));

  ngOnInit() {}
}
