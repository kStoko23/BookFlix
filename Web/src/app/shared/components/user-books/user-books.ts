import {
  Component,
  inject,
  OnInit,
  CUSTOM_ELEMENTS_SCHEMA,
  ElementRef,
  viewChild,
  input,
} from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Book } from '../../../core/models/book.model';
import { register } from 'swiper/element/bundle';
import { BookCard } from '../book-card/book-card';
import { Button } from '../button/button';
import { Loader } from '../loader/loader';

register();

@Component({
  selector: 'app-user-books',
  templateUrl: './user-books.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [BookCard, Button, Loader],
})
export class UserBooks implements OnInit {
  authService = inject(AuthService);
  private swiperRef = viewChild<ElementRef>('swiperRef');

  books = input.required<Book[]>();
  isLoggedIn = input.required<boolean>();
  loading = input.required<boolean>();
  error = input.required<boolean>();

  ngOnInit() {}

  next() {
    this.swiperRef()?.nativeElement.swiper.slideNext();
  }
  prev() {
    this.swiperRef()?.nativeElement.swiper.slidePrev();
  }
}
