import { Component, input, output } from '@angular/core';
import { NgClass, NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';

type ButtonVariant = 'primary' | 'outline' | 'danger';

@Component({
  selector: 'app-button',
  imports: [NgClass, RouterLink, NgTemplateOutlet],
  templateUrl: './button.html',
  styleUrl: './button.css',
})
export class Button {
  variant = input<ButtonVariant>('primary');
  href = input<string>();
  clicked = output<void>();

  variantClasses() {
    const base =
      'flex items-center gap-2 rounded-md px-5 py-2.5 text-lg font-medium transition-colors cursor-pointer';
    const variants: Record<ButtonVariant, string> = {
      primary: 'bg-accent hover:bg-accent-hover text-white',
      outline: 'border border-border text-content-primary hover:border-border-strong',
      danger: 'border border-accent-dark bg-accent-dark/20 hover:bg-accent-dark/40 text-white',
    };
    return `${base} ${variants[this.variant()]}`;
  }
}
