import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QueryBox } from './query-box';

describe('QueryBox', () => {
  let component: QueryBox;
  let fixture: ComponentFixture<QueryBox>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QueryBox]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QueryBox);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
