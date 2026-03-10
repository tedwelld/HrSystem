import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AppNotification } from '../core/models/api.models';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';

type AppRole = 'Admin' | 'Candidate';

interface NavItem {
  label: string;
  path: string;
  icon: string;
  roles?: AppRole[];
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);
  private readonly router = inject(Router);

  readonly user = this.authService.user;
  readonly dashboardLink = computed(() =>
    this.authService.isAdmin() ? '/app/admin-dashboard' : '/app/candidate-dashboard'
  );

  readonly navGroups: NavGroup[] = [
    {
      label: 'Main',
      items: [{ label: 'Dashboard', path: '/app', icon: 'fa-solid fa-chart-pie' }]
    },
    {
      label: 'Recruitment',
      items: [
        { label: 'Jobs', path: '/app/jobs', icon: 'fa-solid fa-briefcase' },
        { label: 'Applications', path: '/app/applications', icon: 'fa-solid fa-file-circle-check' }
      ]
    },
    {
      label: 'Monitoring',
      items: [
        { label: 'Notifications', path: '/app/notifications', icon: 'fa-solid fa-bell' },
        { label: 'Snapshots', path: '/app/snapshots', icon: 'fa-solid fa-camera-retro' }
      ]
    },
    {
      label: 'System',
      items: [
        { label: 'Settings', path: '/app/settings', icon: 'fa-solid fa-sliders' }
      ]
    }
  ];

  readonly currentRole = computed<AppRole>(() => (this.authService.isAdmin() ? 'Admin' : 'Candidate'));
  readonly groupedNav = computed(() => {
    const role = this.currentRole();
    return this.navGroups
      .map((group) => ({
        label: group.label,
        items: group.items
          .map((item) => ({ ...item, path: item.path === '/app' ? this.dashboardLink() : item.path }))
          .filter((item) => !item.roles || item.roles.includes(role))
      }))
      .filter((group) => group.items.length > 0);
  });

  readonly now = signal(new Date());
  readonly sidebarOpen = signal(false);
  readonly isMobile = signal(window.innerWidth < 1080);
  readonly autoHideSidebar = signal(true);
  readonly sidebarHovered = signal(false);
  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount = signal(0);
  readonly showNotificationPanel = signal(false);
  readonly currentPath = signal('');
  readonly sidebarExpanded = computed(() => (this.isMobile() ? this.sidebarOpen() : this.sidebarHovered()));

  private clockHandle?: ReturnType<typeof setInterval>;
  private notificationHandle?: ReturnType<typeof setInterval>;
  private routerEventsSub?: Subscription;

  ngOnInit(): void {
    this.currentPath.set(this.router.url);
    this.loadPreference();
    this.loadUnreadCount();
    this.onResize();

    this.clockHandle = setInterval(() => this.now.set(new Date()), 30000);
    this.notificationHandle = setInterval(() => this.refreshNotifications(), 60000);

    this.routerEventsSub = this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe(() => {
      this.currentPath.set(this.router.url);
      if (window.innerWidth < 1080 || this.autoHideSidebar()) {
        this.sidebarOpen.set(false);
      }
      this.showNotificationPanel.set(false);
    });
  }

  ngOnDestroy(): void {
    if (this.clockHandle) clearInterval(this.clockHandle);
    if (this.notificationHandle) clearInterval(this.notificationHandle);
    this.routerEventsSub?.unsubscribe();
  }

  onResize() {
    const mobile = window.innerWidth < 1080;
    this.isMobile.set(mobile);
    if (mobile) {
      this.sidebarOpen.set(false);
      return;
    }

    this.sidebarOpen.set(false);
  }

  toggleSidebar() {
    if (!this.isMobile()) {
      return;
    }

    this.sidebarOpen.set(!this.sidebarOpen());
  }

  setSidebarHovered(isHovered: boolean) {
    if (this.isMobile()) {
      return;
    }

    this.sidebarHovered.set(isHovered);
  }

  onNavClick() {
    if (window.innerWidth < 1080 || this.autoHideSidebar()) {
      this.sidebarOpen.set(false);
    }
  }

  navigate(path: string) {
    if (this.router.url === path) {
      this.onNavClick();
      this.showNotificationPanel.set(false);
      return;
    }

    this.router.navigateByUrl(path);
  }

  isActive(path: string): boolean {
    const current = this.currentPath();
    if (path.endsWith('dashboard')) {
      return current === path;
    }

    return current === path || current.startsWith(`${path}/`);
  }

  toggleAutoHide() {
    this.autoHideSidebar.set(!this.autoHideSidebar());
    this.persistPreference();
  }

  toggleNotificationPanel() {
    const next = !this.showNotificationPanel();
    this.showNotificationPanel.set(next);
    if (next) {
      this.loadNotifications();
    }
  }

  markAsRead(id: number) {
    this.api.markNotificationAsRead(id).subscribe(() => {
      this.loadNotifications();
      this.loadUnreadCount();
    });
  }

  markAllAsRead() {
    this.api.markAllNotificationsAsRead().subscribe(() => {
      this.loadNotifications();
      this.loadUnreadCount();
    });
  }

  logout() {
    this.authService.logout();
  }

  trackGroup(_: number, group: NavGroup) {
    return group.label;
  }

  trackNav(_: number, item: NavItem) {
    return item.path;
  }

  trackNotification(_: number, item: AppNotification) {
    return item.id;
  }

  private refreshNotifications() {
    if (document.hidden) {
      return;
    }

    if (this.showNotificationPanel()) {
      this.loadNotifications();
      return;
    }

    this.loadUnreadCount();
  }

  private loadNotifications() {
    this.api.getNotifications().subscribe((items) => {
      this.notifications.set(items.slice(0, 8));
      this.unreadCount.set(items.filter((x) => !x.isRead).length);
    });
  }

  private loadUnreadCount() {
    this.api.getUnreadNotificationCount().subscribe({
      next: ({ count }) => this.unreadCount.set(count),
      error: () => this.unreadCount.set(0)
    });
  }

  private loadPreference() {
    this.api.getMyPreference().subscribe({
      next: (pref) => {
        this.autoHideSidebar.set(pref.autoHideSidebar);
        this.applyTheme();
      },
      error: () => {
        this.applyTheme();
      }
    });
  }

  private persistPreference() {
    this.api
      .updateMyPreference({
        theme: 'light',
        autoHideSidebar: this.autoHideSidebar()
      })
      .subscribe();
  }

  private applyTheme() {
    document.body.setAttribute('data-theme', 'light');
  }
}
