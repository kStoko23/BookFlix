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
  private swiperRef = viewChild<ElementRef>('swiperRef');

  category = input<BookCategory>();
  title = input<string>('Books');
  books = input.required<Book[]>();

  filteredBooks = computed(() => {
    const cat = this.category();
    const all = this.books();
    return cat != null ? all.filter((b) => b.category === cat) : all;
  });

  sectionId = computed(() => {
    const cat = this.category();
    return cat != null ? 'category-' + categorySlug(cat) : null;
  });

  next() {
    this.swiperRef()?.nativeElement.swiper.slideNext();
  }
  prev() {
    this.swiperRef()?.nativeElement.swiper.slidePrev();
  }
}
