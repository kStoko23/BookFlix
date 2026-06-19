import { Component, input, output } from '@angular/core';
import { Button } from '../button/button';

@Component({
  selector: 'app-dialog',
  imports: [Button],
  templateUrl: './dialog.html',
  styleUrl: './dialog.css',
})
export class Dialog {
  visible = input<boolean>(false);
  title = input<string>('Are you sure?');
  message = input<string>('');
  confirmLabel = input<string>('Confirm');
  cancelLabel = input<string>('Cancel');

  confirmed = output<void>();
  cancelled = output<void>();

  onConfirm() {
    this.confirmed.emit();
  }
  onCancel() {
    this.cancelled.emit();
  }
  onBackdropClick(event: MouseEvent) {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }
}
