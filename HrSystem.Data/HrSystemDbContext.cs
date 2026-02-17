using HrSystem.Data.EntityModels;
using HrSystem.Data.EntityModels.Enums;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Data;

public class HrSystemDbContext(DbContextOptions<HrSystemDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<CvProfile> CvProfiles => Set<CvProfile>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<FollowUpNote> FollowUpNotes => Set<FollowUpNote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<InterviewSchedule> InterviewSchedules => Set<InterviewSchedule>();
    public DbSet<ApplicationScorecard> ApplicationScorecards => Set<ApplicationScorecard>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>();
            entity.HasMany(x => x.PostedJobs)
                .WithOne(x => x.PostedByAdmin)
                .HasForeignKey(x => x.PostedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Snapshots)
                .WithOne(x => x.ActorUser)
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Preference)
                .WithOne(x => x.User)
                .HasForeignKey<UserPreference>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.InterviewsAsCandidate)
                .WithOne(x => x.Candidate)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.InterviewsAsAdmin)
                .WithOne(x => x.Admin)
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Sessions)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<Snapshot>(entity =>
        {
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.Property(x => x.SalaryMin).HasPrecision(18, 2);
            entity.Property(x => x.SalaryMax).HasPrecision(18, 2);
            entity.HasOne(x => x.Company)
                .WithMany(x => x.JobPostings)
                .HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.Property(x => x.Stage).HasConversion<string>();
            entity.Property(x => x.MatchScore).HasPrecision(5, 2);
            entity.HasIndex(x => new { x.JobPostingId, x.CandidateId }).IsUnique();

            entity.HasOne(x => x.JobPosting)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Candidate)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CvProfile)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.CvProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.InterviewSchedules)
                .WithOne(x => x.JobApplication)
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Scorecard)
                .WithOne(x => x.JobApplication)
                .HasForeignKey<ApplicationScorecard>(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationScorecard>(entity =>
        {
            entity.HasIndex(x => x.JobApplicationId).IsUnique();
            entity.Property(x => x.SkillMatchScore).HasPrecision(5, 2);
            entity.Property(x => x.ExperienceScore).HasPrecision(5, 2);
            entity.Property(x => x.EducationScore).HasPrecision(5, 2);
            entity.Property(x => x.CertificationsScore).HasPrecision(5, 2);
            entity.Property(x => x.OverallScore).HasPrecision(5, 2);
        });

        modelBuilder.Entity<InterviewSchedule>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>();
            entity.HasIndex(x => x.ScheduledStartUtc);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.RefreshTokenHash).IsUnique();
        });

        modelBuilder.Entity<CvProfile>(entity =>
        {
            entity.HasOne(x => x.Candidate)
                .WithMany(x => x.CvProfiles)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FollowUpNote>(entity =>
        {
            entity.HasOne(x => x.JobApplication)
                .WithMany(x => x.FollowUpNotes)
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Admin)
                .WithMany(x => x.FollowUpNotes)
                .HasForeignKey(x => x.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RelatedJobPosting)
                .WithMany()
                .HasForeignKey(x => x.RelatedJobPostingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NotificationDelivery>(entity =>
        {
            entity.Property(x => x.Channel).HasConversion<string>();
            entity.HasOne(x => x.Notification)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = 1,
            Name = "AppIt HR Solutions",
            Address = "100 Main Street",
            City = "Seattle",
            Country = "USA",
            Phone = "+1-206-555-0100",
            Email = "contact@appit-hr.com",
            Description = "Human resources and recruitment company",
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
