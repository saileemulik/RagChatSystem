import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Citation } from '../../models/query.model';

@Component({
  selector: 'app-citation-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './citation-panel.html',
  styleUrl: './citation-panel.css',
})
export class CitationPanel {
  @Input() citations: Citation[] = [];
  
  isExpanded = signal(false);

  togglePanel() {
    this.isExpanded.update(expanded => !expanded);
  }
}
