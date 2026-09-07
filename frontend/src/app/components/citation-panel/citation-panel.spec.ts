import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitationPanel } from './citation-panel';

describe('CitationPanel', () => {
  let component: CitationPanel;
  let fixture: ComponentFixture<CitationPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitationPanel]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CitationPanel);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
