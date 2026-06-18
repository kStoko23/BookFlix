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

register();

@Component({
  selector: 'app-books-slider',
  templateUrl: './books-slider.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
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

  getImageUrl(book: Book): string {
    return `https://picsum.photos/seed/book${book.id}/250/450.webp`;
  }

  navigateToBook(id: number) {
    this.router.navigate(['/books', id]);
  }

  next() {
    this.swiperRef()?.nativeElement.swiper.slideNext();
  }
  prev() {
    this.swiperRef()?.nativeElement.swiper.slidePrev();
  }
}
