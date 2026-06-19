import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../../../core/services/book.service';
import { AuthService } from '../../../core/services/auth.service';
import { Book } from '../../../core/models/book.model';
import { CategoryLabelsPipe } from '../../../shared/pipes/category-labels.pipe';
import { DatePipe } from '@angular/common';
import { Button } from '../../../shared/components/button/button';
import { Dialog } from '../../../shared/components/dialog/dialog';

@Component({
  selector: 'app-book-details',
  imports: [RouterLink, CategoryLabelsPipe, DatePipe, Button, Dialog],
  templateUrl: './book-details.html',
})
export class BookDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private bookService = inject(BookService);
  private authService = inject(AuthService);
  private router = inject(Router);

  book = signal<Book | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  deleting = signal(false);
  deleteDialogOpen = signal(false);

  canManage = computed(() => {
    const book = this.book();
    const userId = this.authService.getUserId();
    return book !== null && userId !== null && book.userId === userId;
  });

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id)) {
      this.error.set('Invalid book id');
      this.loading.set(false);
      return;
    }

    this.bookService.getBook(id).subscribe({
      next: (book) => {
        this.book.set(book);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.status === 404 ? 'Book not found' : 'Failed to load book');
        this.loading.set(false);
      },
    });
  }

  getImageUrl(book: Book): string {
    return `https://picsum.photos/seed/book${book.id}/400/600.webp`;
  }

  openDeleteDialog() {
    this.deleteDialogOpen.set(true);
  }
  cancelDelete() {
    this.deleteDialogOpen.set(false);
  }

  confirmDelete() {
    const book = this.book();
    if (!book) return;

    this.deleting.set(true);
    this.bookService.deleteBook(book.id).subscribe({
      next: () => this.router.navigate(['/my-list']),
      error: () => {
        this.deleting.set(false);
        this.deleteDialogOpen.set(false);
        this.error.set('Failed to delete book');
      },
    });
  }
}
