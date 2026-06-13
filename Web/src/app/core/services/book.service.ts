import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Book, CreateBookRequest, UpdateBookRequest, PagedResponse } from '../models/book.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private readonly apiUrl = environment.apiUrl + '/books';
  private httpClient = inject(HttpClient);

  getBooks(page?: number, pageSize?: number): Observable<PagedResponse<Book>> {
    const params: any = {};
    if (page !== undefined) {
      params.page = page;
    }
    if (pageSize !== undefined) {
      params.pageSize = pageSize;
    }
    return this.httpClient.get<PagedResponse<Book>>(`${this.apiUrl}`, { params });
  }

  getBook(id: number): Observable<Book> {
    return this.httpClient.get<Book>(`${this.apiUrl}/${id}`);
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
