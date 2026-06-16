import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserBooks } from './user-books';

describe('UserBooks', () => {
  let component: UserBooks;
  let fixture: ComponentFixture<UserBooks>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserBooks],
    }).compileComponents();

    fixture = TestBed.createComponent(UserBooks);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
