import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BooksGrid } from './books-grid';

describe('BooksGrid', () => {
  let component: BooksGrid;
  let fixture: ComponentFixture<BooksGrid>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BooksGrid],
    }).compileComponents();

    fixture = TestBed.createComponent(BooksGrid);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
