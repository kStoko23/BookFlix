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
import { BookCard } from '../book-card/book-card';
import { Button } from '../button/button';

register();

@Component({
  selector: 'app-user-books',
  templateUrl: './user-books.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [BookCard, Button],
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

  next() {
    this.swiperRef()?.nativeElement.swiper.slideNext();
  }
  prev() {
    this.swiperRef()?.nativeElement.swiper.slidePrev();
  }
}
