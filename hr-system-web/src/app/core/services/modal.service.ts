import { Injectable, computed, signal } from '@angular/core';

export interface ConfirmModalOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  tone?: 'primary' | 'danger';
}

interface ConfirmModalState {
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel: string;
  tone: 'primary' | 'danger';
  resolve: (result: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ModalService {
  private readonly modalSignal = signal<ConfirmModalState | null>(null);

  readonly modal = computed(() => this.modalSignal());

  confirm(options: ConfirmModalOptions): Promise<boolean> {
    const active = this.modalSignal();
    if (active) {
      active.resolve(false);
      this.modalSignal.set(null);
    }

    return new Promise<boolean>((resolve) => {
      this.modalSignal.set({
        title: options.title,
        message: options.message,
        confirmLabel: options.confirmLabel ?? 'Confirm',
        cancelLabel: options.cancelLabel ?? 'Cancel',
        tone: options.tone ?? 'primary',
        resolve
      });
    });
  }

  close(result: boolean) {
    const active = this.modalSignal();
    if (!active) {
      return;
    }

    this.modalSignal.set(null);
    active.resolve(result);
  }
}
