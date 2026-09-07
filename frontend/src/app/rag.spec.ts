import { TestBed } from '@angular/core/testing';

import { Rag } from './rag';

describe('Rag', () => {
  let service: Rag;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Rag);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
