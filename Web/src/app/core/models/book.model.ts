export type Book = {
  id: number;
  title: string;
  author: string;
  isbn: string;
  pages: number;
  rating: number;
  createdAt: string;
  userId: number;
}

export type PagedResponse<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type CreateBookRequest = {
  title: string;
  author: string;
  isbn: string;
  pages: number;
  rating: number;
}

export type UpdateBookRequest = CreateBookRequest;
