using System.Collections.Generic;
using System.Reflection.Emit;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Fix for Users table triggers
                entity.ToTable("Users", tb => tb.UseSqlOutputClause(false));

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Department).IsRequired().HasMaxLength(200);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasMany(e => e.Claims)
                      .WithOne(e => e.User)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.CoordinatedClaims)
                      .WithOne(e => e.Coordinator)
                      .HasForeignKey(e => e.CoordinatorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ManagedClaims)
                      .WithOne(e => e.Manager)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Claim
            modelBuilder.Entity<Claim>(entity =>
            {
                // --- CHANGE THIS LINE ---
                // Fix for Claims table triggers (The error you just got)
                entity.ToTable("Claims", tb => tb.UseSqlOutputClause(false));
                // ------------------------

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.ClaimNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.WorkDescription).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.HoursWorked).HasColumnType("decimal(18,2)");
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.ReviewerComments).HasMaxLength(1000);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Document
            modelBuilder.Entity<Document>(entity =>
            {
                // It is safe to apply this here as well in case you have triggers on Documents too
                entity.ToTable("Documents", tb => tb.UseSqlOutputClause(false));

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimId);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EncryptedContent).IsRequired();
                entity.Property(e => e.UploadDate).HasDefaultValueSql("GETUTCDATE()");

                // Configure cascade delete for documents when claim is deleted
                entity.HasOne(e => e.Claim)
                      .WithMany(e => e.Documents)
                      .HasForeignKey(e => e.ClaimId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}