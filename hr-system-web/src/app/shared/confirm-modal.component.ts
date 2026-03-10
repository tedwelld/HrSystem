import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { ModalService } from '../core/services/modal.service';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-modal.component.html',
  styleUrl: './confirm-modal.component.scss'
})
export class ConfirmModalComponent {
  private readonly modalService = inject(ModalService);

  readonly modal = this.modalService.modal;
  readonly confirmButtonClass = computed(() =>
    this.modal()?.tone === 'danger' ? 'btn btn-red' : 'btn btn-blue'
  );

  close(result: boolean) {
    this.modalService.close(result);
  }
}
