import {
  Component,
  inject,
  signal,
  OnInit,
  CUSTOM_ELEMENTS_SCHEMA,
  ElementRef,
  viewChild,
} from '@angular/core';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';
import { Book } from '../../../core/models/book.model';
import { Router } from '@angular/router';
import { register } from 'swiper/element/bundle';

register();

@Component({
  selector: 'app-user-books',
  templateUrl: './user-books.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class UserBooks implements OnInit {
  private bookService = inject(BookService);
  authService = inject(AuthService);
  private router = inject(Router);
  private swiperRef = viewChild<ElementRef>('swiperRef');

  books = signal<Book[]>([]);

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.bookService.getMyBooks(1, 20).subscribe({
        next: (res) => this.books.set(res.items),
      });
    }
  }

  getImageUrl(book: Book): string {
    return `https://picsum.photos/seed/book${book.id}/250/600`;
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
