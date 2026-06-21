import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { BookService } from '../../../core/services/book.service';
import { BookCategory, BookCategoryLabels } from '../../../core/models/book.model';
import { Button } from '../../../shared/components/button/button';

const ISBN_REGEX = /^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$/;

function notOnlyWhitespace(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  if (!value) return null;
  return value.trim().length === 0 ? { whitespace: true } : null;
}

@Component({
  selector: 'app-book-form',
  imports: [ReactiveFormsModule, Button],
  templateUrl: './book-form.html',
})
export class BookForm implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookService = inject(BookService);
  private fb = inject(FormBuilder);

  bookId = signal<number | null>(null);
  isEdit = computed(() => this.bookId() !== null);
  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);

  categories = Object.values(BookCategory)
    .filter((v): v is BookCategory => typeof v === 'number')
    .map((value) => ({ value, label: BookCategoryLabels[value] }));

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    author: ['', [Validators.required, Validators.maxLength(200)]],
    isbn: ['', [Validators.required, Validators.pattern(ISBN_REGEX)]],
    pages: [0, [Validators.required, Validators.min(1), Validators.max(10000)]],
    rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
    description: ['', [Validators.maxLength(1200), notOnlyWhitespace]],
    category: [BookCategory.Other, [Validators.required]],
  });

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.bookId.set(id);
      this.loading.set(true);
      this.bookService.getBook(id).subscribe({
        next: (book) => {
          this.form.patchValue({
            title: book.title,
            author: book.author,
            isbn: book.isbn,
            pages: book.pages,
            rating: book.rating,
            description: book.description ?? '',
            category: book.category,
          });
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Failed to load book');
          this.loading.set(false);
        },
      });
    }
  }

  setRating(value: number) {
    this.form.controls.rating.setValue(value);
    this.form.controls.rating.markAsTouched();
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();
    const value = {
      ...raw,
      description: raw.description.trim() || undefined,
    };

    const request = this.isEdit()
      ? this.bookService.updateBook(this.bookId()!, value)
      : this.bookService.createBook(value);

    request.subscribe({
      next: () => this.router.navigate(['/my-list']),
      error: () => {
        this.saving.set(false);
        this.error.set('Failed to save book');
      },
    });
  }
}
