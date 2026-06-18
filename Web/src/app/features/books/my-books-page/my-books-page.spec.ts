import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyBooksPage } from './my-books-page';

describe('MyBooksPage', () => {
  let component: MyBooksPage;
  let fixture: ComponentFixture<MyBooksPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyBooksPage],
    }).compileComponents();

    fixture = TestBed.createComponent(MyBooksPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
