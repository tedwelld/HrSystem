import { Routes } from '@angular/router';
import { AppShellComponent } from './layout/app-shell.component';
import { LoginComponent } from './auth/login.component';
import { RegisterComponent } from './auth/register.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { DashboardRedirectComponent } from './pages/dashboard-redirect.component';
import { CandidateDashboardComponent } from './pages/candidate-dashboard.component';
import { AdminDashboardComponent } from './pages/admin-dashboard.component';
import { JobsComponent } from './pages/jobs.component';
import { ApplicationsComponent } from './pages/applications.component';
import { NotificationsComponent } from './pages/notifications.component';
import { SnapshotsComponent } from './pages/snapshots.component';
import { SettingsComponent } from './pages/settings.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'app',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', component: DashboardRedirectComponent },
      {
        path: 'candidate-dashboard',
        component: CandidateDashboardComponent,
        canActivate: [roleGuard],
        data: { role: 'Candidate' }
      },
      {
        path: 'admin-dashboard',
        component: AdminDashboardComponent,
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      { path: 'jobs', component: JobsComponent },
      { path: 'applications', component: ApplicationsComponent },
      { path: 'notifications', component: NotificationsComponent },
      { path: 'snapshots', component: SnapshotsComponent },
      { path: 'settings', component: SettingsComponent }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
