using System;
using System.Collections.Generic;
using GiftOfTheGivers.Web.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data.Relief;

public partial class ReliefManagementContext : DbContext
{
    public ReliefManagementContext(DbContextOptions<ReliefManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Donor> Donors { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<RecurringDonationSchedule> RecurringDonationSchedules { get; set; }

    public virtual DbSet<ReliefProject> ReliefProjects { get; set; }

    public virtual DbSet<ReliefProjectUpdate> ReliefProjectUpdates { get; set; }

    public virtual DbSet<TaxCertificate> TaxCertificates { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    public virtual DbSet<VolunteerProjectAssignment> VolunteerProjectAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.AuditLogId).HasColumnName("AuditLogID");
            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AuditLogs_CreatedAt");
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.EntityId)
                .HasMaxLength(100)
                .HasColumnName("EntityID");
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyCode);

            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CurrencyName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Currencies_IsActive");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasIndex(e => e.DonationDate, "IX_Donations_DonationDate");

            entity.HasIndex(e => e.DonorId, "IX_Donations_DonorID");

            entity.HasIndex(e => new { e.ReliefProjectId, e.DonationDate }, "IX_Donations_ReliefProject_Date");

            entity.Property(e => e.DonationId).HasColumnName("DonationID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DonationDate).HasDefaultValueSql("(sysutcdatetime())", "DF_Donations_DonationDate");
            entity.Property(e => e.DonationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending", "DF_Donations_Status");
            entity.Property(e => e.DonationType).HasMaxLength(20);
            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.RecurringScheduleId).HasColumnName("RecurringScheduleID");
            entity.Property(e => e.ReliefProjectId).HasColumnName("ReliefProjectID");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.Donations)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donations_Currencies");

            entity.HasOne(d => d.Donor).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DonorId)
                .HasConstraintName("FK_Donations_Donors");

            entity.HasOne(d => d.RecurringSchedule).WithMany(p => p.Donations)
                .HasForeignKey(d => d.RecurringScheduleId)
                .HasConstraintName("FK_Donations_RecurringSchedules");

            entity.HasOne(d => d.ReliefProject).WithMany(p => p.Donations)
                .HasForeignKey(d => d.ReliefProjectId)
                .HasConstraintName("FK_Donations_ReliefProjects");
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Donors_UserID").IsUnique();

            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Donors_CreatedAt");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Donor)
                .HasForeignKey<Donor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donors_Users");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.UserId, "UQ_Employees_UserID").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Employees_CreatedAt");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Users");
        });

        modelBuilder.Entity<RecurringDonationSchedule>(entity =>
        {
            entity.HasKey(e => e.RecurringScheduleId);

            entity.HasIndex(e => new { e.IsActive, e.NextDonationDate }, "IX_RecurringSchedules_Active_NextDate");

            entity.Property(e => e.RecurringScheduleId).HasColumnName("RecurringScheduleID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RecurringSchedules_CreatedAt");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.Frequency).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_RecurringSchedules_IsActive");
            entity.Property(e => e.ReliefProjectId).HasColumnName("ReliefProjectID");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.RecurringDonationSchedules)
                .HasForeignKey(d => d.CurrencyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecurringSchedules_Currencies");

            entity.HasOne(d => d.Donor).WithMany(p => p.RecurringDonationSchedules)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecurringSchedules_Donors");

            entity.HasOne(d => d.ReliefProject).WithMany(p => p.RecurringDonationSchedules)
                .HasForeignKey(d => d.ReliefProjectId)
                .HasConstraintName("FK_RecurringSchedules_ReliefProjects");
        });

        modelBuilder.Entity<ReliefProject>(entity =>
        {
            entity.HasIndex(e => e.ProjectStatus, "IX_ReliefProjects_ProjectStatus");

            entity.Property(e => e.ReliefProjectId).HasColumnName("ReliefProjectID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_ReliefProjects_CreatedAt");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("CreatedByEmployeeID");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ProjectName).HasMaxLength(200);
            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Planned", "DF_ReliefProjects_Status");

            entity.HasOne(d => d.CreatedByEmployee).WithMany(p => p.ReliefProjects)
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReliefProjects_Employees");
        });

        modelBuilder.Entity<ReliefProjectUpdate>(entity =>
        {
            entity.HasKey(e => e.ProjectUpdateId);

            entity.HasIndex(e => new { e.ReliefProjectId, e.PostedAt }, "IX_ReliefProjectUpdates_Project_PostedAt").IsDescending(false, true);

            entity.Property(e => e.ProjectUpdateId).HasColumnName("ProjectUpdateID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.PostedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_ReliefProjectUpdates_PostedAt");
            entity.Property(e => e.ReliefProjectId).HasColumnName("ReliefProjectID");
            entity.Property(e => e.UpdateTitle).HasMaxLength(200);

            entity.HasOne(d => d.Employee).WithMany(p => p.ReliefProjectUpdates)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReliefProjectUpdates_Employees");

            entity.HasOne(d => d.ReliefProject).WithMany(p => p.ReliefProjectUpdates)
                .HasForeignKey(d => d.ReliefProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReliefProjectUpdates_ReliefProjects");
        });

        modelBuilder.Entity<TaxCertificate>(entity =>
        {
            entity.HasIndex(e => e.CertificateNumber, "UQ_TaxCertificates_CertificateNumber").IsUnique();

            entity.HasIndex(e => e.DonationId, "UQ_TaxCertificates_DonationID").IsUnique();

            entity.Property(e => e.TaxCertificateId).HasColumnName("TaxCertificateID");
            entity.Property(e => e.CertificateNumber).HasMaxLength(50);
            entity.Property(e => e.CertificateStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Generated", "DF_TaxCertificates_Status");
            entity.Property(e => e.DonationId).HasColumnName("DonationID");
            entity.Property(e => e.FileReference).HasMaxLength(500);
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TaxCertificates_GeneratedAt");

            entity.HasOne(d => d.Donation).WithOne(p => p.TaxCertificate)
                .HasForeignKey<TaxCertificate>(d => d.DonationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaxCertificates_Donations");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Users_CreatedAt");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.UserRole).HasMaxLength(20);
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasIndex(e => e.VolunteerStatus, "IX_Volunteers_VolunteerStatus");

            entity.Property(e => e.VolunteerId).HasColumnName("VolunteerID");
            entity.Property(e => e.Availability).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(sysutcdatetime())", "DF_Volunteers_RegistrationDate");
            entity.Property(e => e.Skills).HasMaxLength(500);
            entity.Property(e => e.VolunteerStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Pending", "DF_Volunteers_Status");
        });

        modelBuilder.Entity<VolunteerProjectAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);

            entity.HasIndex(e => new { e.VolunteerId, e.ReliefProjectId }, "UQ_VolunteerProjectAssignments").IsUnique();

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_VolunteerAssignments_AssignedAt");
            entity.Property(e => e.AssignedByEmployeeId).HasColumnName("AssignedByEmployeeID");
            entity.Property(e => e.AssignmentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Assigned", "DF_VolunteerAssignments_Status");
            entity.Property(e => e.ReliefProjectId).HasColumnName("ReliefProjectID");
            entity.Property(e => e.VolunteerId).HasColumnName("VolunteerID");

            entity.HasOne(d => d.AssignedByEmployee).WithMany(p => p.VolunteerProjectAssignments)
                .HasForeignKey(d => d.AssignedByEmployeeId)
                .HasConstraintName("FK_VolunteerAssignments_Employees");

            entity.HasOne(d => d.ReliefProject).WithMany(p => p.VolunteerProjectAssignments)
                .HasForeignKey(d => d.ReliefProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VolunteerAssignments_Projects");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.VolunteerProjectAssignments)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VolunteerAssignments_Volunteers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
