import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllBooksPage } from './all-books-page';

describe('AllBooksPage', () => {
  let component: AllBooksPage;
  let fixture: ComponentFixture<AllBooksPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllBooksPage],
    }).compileComponents();

    fixture = TestBed.createComponent(AllBooksPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
