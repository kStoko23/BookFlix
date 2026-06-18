import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BookFilters } from './book-filters';

describe('BookFilters', () => {
  let component: BookFilters;
  let fixture: ComponentFixture<BookFilters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookFilters],
    }).compileComponents();

    fixture = TestBed.createComponent(BookFilters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
