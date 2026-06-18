import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import {
  Book,
  CreateBookRequest,
  UpdateBookRequest,
  PagedResponse,
  BookCategory,
} from '../models/book.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private readonly apiUrl = environment.apiUrl + '/books';
  private httpClient = inject(HttpClient);

  getBooks(
    page?: number,
    pageSize?: number,
    search?: string,
    category?: BookCategory,
  ): Observable<PagedResponse<Book>> {
    const params: any = {};
    if (page !== undefined) {
      params.page = page;
    }
    if (pageSize !== undefined) {
      params.pageSize = pageSize;
    }
    if (search !== undefined) {
      params.search = search;
    }
    if (category !== undefined) {
      params.category = category;
    }
    return this.httpClient.get<PagedResponse<Book>>(`${this.apiUrl}`, { params });
  }

  getBook(id: number): Observable<Book> {
    return this.httpClient.get<Book>(`${this.apiUrl}/${id}`);
  }

  getMyBooks(page?: number, pageSize?: number, search?: string): Observable<PagedResponse<Book>> {
    const params: any = {};

    if (page !== undefined) {
      params.page = page;
    }
    if (pageSize !== undefined) {
      params.pageSize = pageSize;
    }
    if (search !== undefined) {
      params.search = search;
    }

    return this.httpClient.get<PagedResponse<Book>>(`${this.apiUrl}/mine`, { params });
  }

  getBooksFromEachCategory(page?: number, pageSize?: number): Observable<PagedResponse<Book>> {
    const params: any = {};
    if (page !== undefined) {
      params.page = page;
    }
    if (pageSize !== undefined) {
      params.pageSize = pageSize;
    }
    return this.httpClient.get<PagedResponse<Book>>(`${this.apiUrl}/categorized`, { params });
  }

  createBook(book: CreateBookRequest): Observable<Book> {
    return this.httpClient.post<Book>(`${this.apiUrl}`, book);
  }

  updateBook(id: number, book: UpdateBookRequest): Observable<Book> {
    return this.httpClient.put<Book>(`${this.apiUrl}/${id}`, book);
  }

  deleteBook(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${this.apiUrl}/${id}`);
  }
}
