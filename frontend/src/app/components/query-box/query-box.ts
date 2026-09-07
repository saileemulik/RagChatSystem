import { Component, EventEmitter, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-query-box',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './query-box.html',
  styleUrl: './query-box.css',
})
export class QueryBox {
  @Output() querySubmitted = new EventEmitter<string>();

  question = '';
  isLoading = signal(false);

  onSubmit() {
    if (this.question.trim()) {
      this.querySubmitted.emit(this.question);
      this.question = '';
    }
  }

  setLoading(loading: boolean) {
    this.isLoading.set(loading);
  }
}
