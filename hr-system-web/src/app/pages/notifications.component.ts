import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HrApiService } from '../core/services/hr-api.service';
import { AppNotification } from '../core/models/api.models';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  private readonly api = inject(HrApiService);

  items: AppNotification[] = [];

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.api.getNotifications().subscribe((data) => {
      this.items = data;
    });
  }

  markRead(notificationId: number) {
    this.api.markNotificationAsRead(notificationId).subscribe(() => this.load());
  }

  markAllRead() {
    this.api.markAllNotificationsAsRead().subscribe(() => this.load());
  }
}
