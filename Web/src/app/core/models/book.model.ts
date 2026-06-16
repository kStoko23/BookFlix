export type Book = {
  id: number;
  title: string;
  author: string;
  description?: string | null;
  category: BookCategory;
  isbn: string;
  pages: number;
  rating: number;
  createdAt: string;
  userId: number;
}

export enum BookCategory {
  NonFiction = 0,
  Fiction = 1,
  Fantasy = 2,
  ScienceFiction = 3,
  Thriller = 4,
  Horror = 5,
  Romance = 6,
  Biography = 7,
  History = 8,
  Educational = 9,
  Other = 10,
}

export const BookCategoryLabels: Record<BookCategory, string> = {
  [BookCategory.NonFiction]: 'Non-Fiction',
  [BookCategory.Fiction]: 'Fiction',
  [BookCategory.Fantasy]: 'Fantasy',
  [BookCategory.ScienceFiction]: 'Science Fiction',
  [BookCategory.Thriller]: 'Thriller',
  [BookCategory.Horror]: 'Horror',
  [BookCategory.Romance]: 'Romance',
  [BookCategory.History]: 'History',
  [BookCategory.Biography]: 'Biography',
  [BookCategory.Educational]: 'Educational',
  [BookCategory.Other]: 'Other',
};

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
  description?: string | null;
  category: BookCategory;
}

export type UpdateBookRequest = CreateBookRequest;
