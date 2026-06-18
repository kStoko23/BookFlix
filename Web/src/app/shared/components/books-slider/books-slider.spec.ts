import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BooksSlider } from './books-slider';

describe('BooksSlider', () => {
  let component: BooksSlider;
  let fixture: ComponentFixture<BooksSlider>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BooksSlider],
    }).compileComponents();

    fixture = TestBed.createComponent(BooksSlider);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
