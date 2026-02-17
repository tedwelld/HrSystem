import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  AdminDashboard,
  AdminCompany,
  AdminEmailSendResult,
  AdminUser,
  AppNotification,
  CandidateDashboard,
  CvProfile,
  JobApplication,
  JobPosting,
  Snapshot,
  UserProfile,
  UserPreference
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class HrApiService {
  private readonly http = inject(HttpClient);

  getOpenJobs() {
    return this.http.get<JobPosting[]>(`${environment.apiBaseUrl}/jobs/open`);
  }

  getMyProfile() {
    return this.http.get<UserProfile>(`${environment.apiBaseUrl}/auth/me`);
  }

  updateMyProfile(payload: { firstName: string; lastName: string; phoneNumber: string }) {
    return this.http.put<UserProfile>(`${environment.apiBaseUrl}/auth/me`, payload);
  }

  getAllJobs() {
    return this.http.get<JobPosting[]>(`${environment.apiBaseUrl}/jobs/all`);
  }

  createJob(payload: {
    title: string;
    description: string;
    location: string;
    employmentType: string;
    experienceLevel: string;
    requiredSkills: string[];
    salaryMin?: number;
    salaryMax?: number;
    companyId?: number;
  }) {
    return this.http.post<JobPosting>(`${environment.apiBaseUrl}/jobs`, payload);
  }

  updateJob(jobId: number, payload: {
    title?: string;
    description?: string;
    location?: string;
    employmentType?: string;
    experienceLevel?: string;
    requiredSkills?: string[];
    salaryMin?: number;
    salaryMax?: number;
    isOpen?: boolean;
  }) {
    return this.http.put(`${environment.apiBaseUrl}/jobs/${jobId}`, payload);
  }

  closeJob(jobId: number) {
    return this.http.post(`${environment.apiBaseUrl}/jobs/${jobId}/close`, {});
  }

  deleteJob(jobId: number) {
    return this.http.delete(`${environment.apiBaseUrl}/jobs/${jobId}`);
  }

  applyForJob(payload: { jobPostingId: number; cvProfileId?: number; coverLetter: string }) {
    return this.http.post<JobApplication>(`${environment.apiBaseUrl}/applications`, payload);
  }

  getMyApplications() {
    return this.http.get<JobApplication[]>(`${environment.apiBaseUrl}/applications/mine`);
  }

  getAllApplications() {
    return this.http.get<JobApplication[]>(`${environment.apiBaseUrl}/applications/admin/all`);
  }

  updateApplicationStage(payload: { applicationId: number; stage: string }) {
    return this.http.post(`${environment.apiBaseUrl}/applications/admin/update-stage`, payload);
  }

  addFollowUp(payload: { applicationId: number; note: string }) {
    return this.http.post(`${environment.apiBaseUrl}/applications/admin/follow-up`, payload);
  }

  getCvProfiles() {
    return this.http.get<CvProfile[]>(`${environment.apiBaseUrl}/cv/mine`);
  }

  uploadStructuredCv(payload: {
    fileName: string;
    fullText: string;
    skills: string[];
    educationSummary: string;
    yearsOfExperience: number;
    certificationsSummary: string;
  }) {
    return this.http.post<CvProfile>(`${environment.apiBaseUrl}/cv/structured`, payload);
  }

  uploadTextCv(file: File) {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post<CvProfile>(`${environment.apiBaseUrl}/cv/text-upload`, formData);
  }

  getNotifications() {
    return this.http.get<AppNotification[]>(`${environment.apiBaseUrl}/notifications/mine`);
  }

  getUnreadNotificationCount() {
    return this.http.get<{ count: number }>(`${environment.apiBaseUrl}/notifications/unread-count`);
  }

  markNotificationAsRead(id: number) {
    return this.http.post(`${environment.apiBaseUrl}/notifications/${id}/read`, {});
  }

  markAllNotificationsAsRead() {
    return this.http.post<{ updated: number }>(`${environment.apiBaseUrl}/notifications/read-all`, {});
  }

  getAdminSnapshots(count = 100) {
    return this.http.get<Snapshot[]>(`${environment.apiBaseUrl}/snapshots/admin/latest?count=${count}`);
  }

  getMySnapshots(count = 100) {
    return this.http.get<Snapshot[]>(`${environment.apiBaseUrl}/snapshots/mine?count=${count}`);
  }

  getMyPreference() {
    return this.http.get<UserPreference>(`${environment.apiBaseUrl}/preferences/me`);
  }

  updateMyPreference(payload: { theme: string; autoHideSidebar: boolean }) {
    return this.http.put<UserPreference>(`${environment.apiBaseUrl}/preferences/me`, payload);
  }

  getAdminDashboard() {
    return this.http.get<AdminDashboard>(`${environment.apiBaseUrl}/dashboard/admin`);
  }

  getCandidateDashboard() {
    return this.http.get<CandidateDashboard>(`${environment.apiBaseUrl}/dashboard/candidate`);
  }

  getAdminUsers() {
    return this.http.get<AdminUser[]>(`${environment.apiBaseUrl}/admin/management/users`);
  }

  updateAdminUser(userId: number, payload: {
    firstName: string;
    lastName: string;
    phoneNumber: string;
    role: string;
    isActive: boolean;
  }) {
    return this.http.put<AdminUser>(`${environment.apiBaseUrl}/admin/management/users/${userId}`, payload);
  }

  getAdminCompanies() {
    return this.http.get<AdminCompany[]>(`${environment.apiBaseUrl}/admin/management/companies`);
  }

  updateAdminCompany(companyId: number, payload: {
    name: string;
    address: string;
    city: string;
    country: string;
    phone: string;
    email: string;
    description: string;
  }) {
    return this.http.put<AdminCompany>(`${environment.apiBaseUrl}/admin/management/companies/${companyId}`, payload);
  }

  sendAdminEmail(payload: { userIds: number[]; includeAllCandidates: boolean; subject: string; message: string }) {
    return this.http.post<AdminEmailSendResult>(`${environment.apiBaseUrl}/admin/management/send-email`, payload);
  }
}
