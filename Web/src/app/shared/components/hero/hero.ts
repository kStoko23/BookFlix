import { Component, inject, input, signal } from '@angular/core';
import { BookService } from '../../../core/services/book.service';
import { Router } from '@angular/router';
import { Book } from '../../../core/models/book.model';
import { Button } from '../button/button';
import { CategoryLabelsPipe } from '../../pipes/category-labels.pipe';
import { DescriptionLengthPipe } from '../../pipes/description-length.pipe';

@Component({
  selector: 'app-hero',
  imports: [Button, CategoryLabelsPipe, DescriptionLengthPipe],
  templateUrl: './hero.html',
  styleUrl: './hero.css',
})
export class Hero {
  private router = inject(Router);

  books = input.required<Book[]>();
  currentIndex = signal<number>(0);
  private timer?: ReturnType<typeof setInterval>;

  ngOnInit() {}

  getImageUrl(book: Book): string {
    return `https://picsum.photos/seed/book${book.id}/1600/1000.webp`;
  }

  goToSlide(index: number) {
    this.currentIndex.set(index);
    this.stopTimer();
  }

  changeSlide(dir: number) {
    this.goToSlide((this.currentIndex() + dir + this.books().length) % this.books().length);
  }

  navigateToBook(id: number) {
    this.router.navigate(['/books', id]);
  }

  private startTimer() {
    this.timer = setInterval(() => this.changeSlide(1), 8000);
  }

  private stopTimer() {
    if (this.timer) {
      clearInterval(this.timer);
      this.startTimer();
    }
  }

  ngOnDestroy() {
    clearInterval(this.timer);
  }
}
