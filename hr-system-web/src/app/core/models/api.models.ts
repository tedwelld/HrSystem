export interface UserProfile {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: 'Admin' | 'Candidate';
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  user: UserProfile;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
  role: string;
  adminInviteCode?: string;
}

export interface JobPosting {
  id: number;
  companyId: number;
  companyName: string;
  postedByAdminId: number;
  title: string;
  description: string;
  location: string;
  employmentType: string;
  experienceLevel: string;
  requiredSkills: string[];
  salaryMin?: number;
  salaryMax?: number;
  isOpen: boolean;
  createdAtUtc: string;
}

export interface CvProfile {
  id: number;
  originalFileName: string;
  skills: string[];
  educationSummary: string;
  yearsOfExperience: number;
  certificationsSummary: string;
  createdAtUtc: string;
}

export interface JobApplication {
  id: number;
  jobPostingId: number;
  jobTitle: string;
  candidateId: number;
  candidateName: string;
  candidateEmail: string;
  cvProfileId?: number;
  stage: string;
  coverLetter: string;
  strengthsSummary: string;
  weaknessesSummary: string;
  matchScore: number;
  submittedAtUtc: string;
  followUpNotes: FollowUpNote[];
}

export interface FollowUpNote {
  id: number;
  adminId: number;
  adminName: string;
  note: string;
  createdAtUtc: string;
}

export interface AppNotification {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAtUtc: string;
}

export interface Snapshot {
  id: number;
  actorUserId?: number;
  actorName: string;
  source: string;
  action: string;
  category: string;
  relatedEntityId?: number;
  details: string;
  createdAtUtc: string;
}

export interface UserPreference {
  userId: number;
  theme: 'light' | 'dark';
  autoHideSidebar: boolean;
  updatedAtUtc: string;
}

export interface AdminUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: 'Admin' | 'Candidate';
  isActive: boolean;
  createdAtUtc: string;
}

export interface AdminCompany {
  id: number;
  name: string;
  address: string;
  city: string;
  country: string;
  phone: string;
  email: string;
  description: string;
}

export interface AdminEmailSendResult {
  requestedRecipients: number;
  successfullySent: number;
  failed: number;
}

export interface ChartPoint {
  label: string;
  value: number;
}

export interface AdminDashboard {
  totalCandidates: number;
  totalAdmins: number;
  totalCompanies: number;
  openJobPostings: number;
  closedJobPostings: number;
  totalApplications: number;
  pendingReviewApplications: number;
  totalInterviewsScheduled: number;
  averageApplicationMatchScore: number;
  jobPostsByMonth: ChartPoint[];
  applicationsByMonth: ChartPoint[];
  applicationsByStatus: ChartPoint[];
}

export interface CandidateDashboard {
  openJobPostings: number;
  myApplications: number;
  interviewScheduled: number;
  notificationsUnread: number;
  myApplicationsByStatus: ChartPoint[];
}
