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
  const colors = ['1a3a5c', '7c1f2e', '2d5a27', '3d3060', '5c3a1a', '1a4a4a', '4a1a1a'];
  const color = colors[book.id % colors.length];
  const title = book.title.length > 20 ? book.title.slice(0, 18) + '…' : book.title;
  const author = book.author.split(' ').slice(0, 2).join(' ');

  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" width="250" height="400" viewBox="0 0 250 400">
      <rect width="250" height="400" rx="3" fill="#${color}"/>
      <rect x="0" y="0" width="8" height="400" fill="rgba(0,0,0,0.2)"/>
      <rect x="15" y="15" width="220" height="370" rx="2" fill="none" stroke="rgba(255,255,255,0.1)" stroke-width="0.5"/>
      <line x1="20" y1="280" x2="230" y2="280" stroke="rgba(255,255,255,0.2)" stroke-width="0.5"/>
      <text x="125" y="240" text-anchor="middle" font-family="Georgia,serif" font-size="16" font-weight="700" fill="rgba(255,255,255,0.95)">${title}</text>
      <text x="125" y="310" text-anchor="middle" font-family="Georgia,serif" font-size="12" fill="rgba(255,255,255,0.6)">${author}</text>
    </svg>`;

  return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
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
