import {
  Component,
  input,
  CUSTOM_ELEMENTS_SCHEMA,
  ElementRef,
  viewChild,
  computed,
} from '@angular/core';
import { Book, BookCategory, categorySlug } from '../../../core/models/book.model';
import { register } from 'swiper/element/bundle';
import { BookCard } from '../book-card/book-card';
import { Loader } from '../loader/loader';

register();

@Component({
  selector: 'app-books-slider',
  templateUrl: './books-slider.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [BookCard, Loader],
})
export class BooksSlider {
  private swiperRef = viewChild<ElementRef>('swiperRef');

  category = input<BookCategory>();
  title = input<string>('Books');
  id = input<string>();
  books = input.required<Book[]>();
  loading = input<boolean>(true);
  error = input.required<boolean>();

  filteredBooks = computed(() => {
    const cat = this.category();
    const all = this.books();
    return cat != null ? all.filter((b) => b.category === cat) : all;
  });

  sectionId = computed(() => {
    const explicit = this.id();
    if (explicit) return explicit;
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
