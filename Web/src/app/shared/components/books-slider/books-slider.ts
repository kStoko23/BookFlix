import {
  Component,
  inject,
  signal,
  input,
  effect,
  CUSTOM_ELEMENTS_SCHEMA,
  ElementRef,
  viewChild,
  computed,
} from '@angular/core';
import { BookService } from '../../../core/services/book.service';
import { Book, BookCategory, categorySlug } from '../../../core/models/book.model';
import { Router } from '@angular/router';
import { register } from 'swiper/element/bundle';
import { BookCard } from '../book-card/book-card';

register();

@Component({
  selector: 'app-books-slider',
  templateUrl: './books-slider.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [BookCard],
})
export class BooksSlider {
  private bookService = inject(BookService);
  private router = inject(Router);
  private swiperRef = viewChild<ElementRef>('swiperRef');

  category = input<BookCategory>();
  title = input<string>('Books');

  books = signal<Book[]>([]);

  sectionId = computed(() => {
    const cat = this.category();
    return cat != null ? 'category-' + categorySlug(cat) : null;
  });

  constructor() {
    effect(() => {
      const cat = this.category();
      this.bookService.getBooks(1, 20, cat).subscribe({
        next: (res) => this.books.set(res.items),
      });
    });
  }

  next() {
    this.swiperRef()?.nativeElement.swiper.slideNext();
  }
  prev() {
    this.swiperRef()?.nativeElement.swiper.slidePrev();
  }
}
