import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { QueryResponse } from '../../models/query.model';

@Component({
  selector: 'app-result-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './result-view.html',
  styleUrl: './result-view.css',
})
export class ResultView {
  @Input() responses: QueryResponse[] = [];
  @Input() isLoading = false;

  formatAnswer(answer: string): any {
    return this.analyzeAndFormat(answer);
  }

  private analyzeAndFormat(answer: string): any {
    if (this.isComparison(answer)) {
      return { type: 'comparison', content: this.extractComparisonData(answer) };
    }
    if (this.isStepByStep(answer)) {
      return { type: 'steps', content: this.extractSteps(answer) };
    }
    if (this.isCalculation(answer)) {
      return { type: 'calculation', content: this.extractCalculation(answer) };
    }
    return { type: 'paragraph', content: this.formatParagraphs(answer) };
  }

  private isComparison(answer: string): boolean {
    const keywords = ['compare', 'vs', 'versus', 'difference', 'better', 'comparison'];
    return keywords.some(k => answer.toLowerCase().includes(k));
  }

  private isStepByStep(answer: string): boolean {
    return /step\s*\d+|\d+\.|guide|process|procedure/gi.test(answer);
  }

  private isCalculation(answer: string): boolean {
    return /\d+\s*[+\-*/]\s*\d+|calculate|sum|result:|total/gi.test(answer);
  }

  private extractComparisonData(answer: string): any {
    const lines = answer.split('\n').filter(line => line.trim());
    const tableData = {
      headers: ['Feature', 'Details'],
      rows: [] as string[][]
    };
    
    // Extract comparison points
    lines.forEach(line => {
      if (line.includes('Azure') || line.includes('vs') || line.includes('-')) {
        const cleanLine = line.replace(/^[-*•]\s*/, '').trim();
        if (cleanLine.length > 10) {
          tableData.rows.push(['Comparison Point', cleanLine.substring(0, 80) + (cleanLine.length > 80 ? '...' : '')]);
        }
      }
    });
    
    if (tableData.rows.length === 0) {
      tableData.rows.push(['Analysis', answer.substring(0, 100) + '...']);
    }
    
    return tableData;
  }

  private extractSteps(answer: string): string[] {
    const lines = answer.split('\n');
    const steps: string[] = [];
    
    lines.forEach(line => {
      const trimmed = line.trim();
      if (/^\d+\.|^step\s*\d+|^-\s|^\*\s|^•\s/gi.test(trimmed)) {
        const cleanStep = trimmed.replace(/^\d+\.\s*|^step\s*\d+:?\s*|^-\s*|^\*\s*|^•\s*/gi, '');
        if (cleanStep.length > 5) {
          steps.push(cleanStep);
        }
      }
    });
    
    // If no clear steps found, create from sentences
    if (steps.length === 0) {
      const sentences = answer.split('.').filter(s => s.trim().length > 15);
      return sentences.slice(0, 5).map((s, i) => `${s.trim()}.`);
    }
    
    return steps;
  }

  private extractCalculation(answer: string): any {
    const calcMatch = answer.match(/(\d+\s*[+\-*/]\s*\d+\s*=\s*\d+)/g);
    const resultMatch = answer.match(/result:?\s*(\d+)|answer:?\s*(\d+)|=\s*(\d+)/gi);
    
    return {
      calculation: calcMatch ? calcMatch[0] : 'Mathematical calculation performed',
      result: resultMatch ? resultMatch[0] : '',
      explanation: answer
    };
  }

  private formatParagraphs(answer: string): string[] {
    return answer.split('\n\n')
      .filter(p => p.trim().length > 0)
      .map(p => p.trim());
  }
}