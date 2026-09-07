import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResultView } from './result-view';

describe('ResultView', () => {
  let component: ResultView;
  let fixture: ComponentFixture<ResultView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResultView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ResultView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
