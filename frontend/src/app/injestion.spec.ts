import { TestBed } from '@angular/core/testing';

import { Injestion } from './injestion';

describe('Injestion', () => {
  let service: Injestion;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Injestion);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
