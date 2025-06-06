using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<ProjectProposal> ProjectProposal { get; set; }
        public DbSet<ProjectApprovalStep> ProjectApprovalStep { get; set; }
        public DbSet<ApprovalRule> ApprovalRule { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Area> Area { get; set; }
        public DbSet<ProjectType> ProjectType { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatus { get; set; }
        public DbSet<ApproverRole> ApproverRole { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-ALEJO;Database=ADMProyectos;Trusted_Connection=True;TrustServerCertificate=true;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Area");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(25).IsRequired();
                entity.HasMany<ProjectProposal>(a => a.ProjectProposals).WithOne(pp => pp.AreaNavigation).HasForeignKey(pp => pp.Area);
                entity.HasMany<ApprovalRule>(a => a.ApprovalRules).WithOne(pp => pp.AreaNavigation).HasForeignKey(pp => pp.Area);
            });
            modelBuilder.ApplyConfiguration(new AreaData());

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(25).IsRequired();
                entity.Property(a => a.Email).HasMaxLength(100).IsRequired();
                entity.HasMany<ProjectProposal>(a => a.ProjectProposals).WithOne(pp => pp.User).HasForeignKey(pp => pp.CreateBy).IsRequired();
                entity.HasMany<ProjectApprovalStep>(a => a.ProjectApprovalSteps).WithOne(pp => pp.User).HasForeignKey(pp => pp.ApproverUserId).IsRequired();
                entity.HasOne(a => a.ApproverRole).WithMany(c => c.Users).HasForeignKey(p => p.Role).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new UserData());

            modelBuilder.Entity<ProjectType>(entity =>
            {
                entity.ToTable("ProjectType");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(25).IsRequired();
                entity.HasMany<ProjectProposal>(a => a.ProjectProposals).WithOne(pp => pp.ProjectType).HasForeignKey(pp => pp.Type).IsRequired();
                entity.HasMany<ApprovalRule>(a => a.ApprovalRules).WithOne(pp => pp.ProjectType).HasForeignKey(pp => pp.Type).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ProjectTypeData());

            modelBuilder.Entity<ApproverRole>(entity =>
            {
                entity.ToTable("ApproverRole");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(25).IsRequired();
                entity.HasMany<ProjectApprovalStep>(a => a.ProjectApprovalSteps).WithOne(pp => pp.ApproverRole).HasForeignKey(pp => pp.ApproverRoleId).IsRequired();
                entity.HasMany<User>(a => a.Users).WithOne(pp => pp.ApproverRole).HasForeignKey(pp => pp.Role).IsRequired();
                entity.HasMany<ApprovalRule>(a => a.ApprovalRules).WithOne(pp => pp.ApproverRole).HasForeignKey(pp => pp.ApproverRoleId).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ApproverRoleData());

            modelBuilder.Entity<ApprovalStatus>(entity =>
            {
                entity.ToTable("ApprovalStatus");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(25).IsRequired();
                entity.HasMany<ProjectProposal>(a => a.ProjectProposals).WithOne(pp => pp.ApprovalStatus).HasForeignKey(pp => pp.Status).IsRequired();
                entity.HasMany<ProjectApprovalStep>(a => a.ProjectApprovalSteps).WithOne(pp => pp.ApprovalStatus).HasForeignKey(pp => pp.Status).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ApprovalStatusData());
            modelBuilder.Entity<ApprovalRule>(entity =>
            {
                entity.ToTable("ApprovalRule");
                entity.HasKey(ar => ar.Id);
                entity.Property(ar => ar.MinAmount).IsRequired();
                entity.Property(ar => ar.MaxAmount).IsRequired();
                entity.Property(ar => ar.MinAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(ar => ar.MaxAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(ar => ar.StepOrder).IsRequired();
                entity.HasOne(ar => ar.AreaNavigation).WithMany(a => a.ApprovalRules).HasForeignKey(ar => ar.Area).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ar => ar.ProjectType).WithMany(pt => pt.ApprovalRules).HasForeignKey(ar => ar.Type).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ar => ar.ApproverRole).WithMany(arole => arole.ApprovalRules).HasForeignKey(ar => ar.ApproverRoleId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.ApplyConfiguration(new ApprovalRuleData());
            modelBuilder.Entity<ProjectProposal>(entity =>
            {
                entity.ToTable("ProjectProposal");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title).HasMaxLength(255).IsRequired();
                entity.Property(a => a.Description).IsRequired();
                entity.Property(a => a.EstimatedAmount).IsRequired();
                entity.Property(p => p.EstimatedAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(a => a.EstimatedDuration).IsRequired();
                entity.Property(a => a.CreatedAt).IsRequired();
                entity.HasOne(p => p.AreaNavigation).WithMany(c => c.ProjectProposals).HasForeignKey(p => p.Area).IsRequired();
                entity.HasOne(p => p.ProjectType).WithMany(c => c.ProjectProposals).HasForeignKey(p => p.Type).IsRequired();
                entity.HasOne(p => p.ApprovalStatus).WithMany(c => c.ProjectProposals).HasForeignKey(p => p.Status).IsRequired();
                entity.HasOne(p => p.User).WithMany(c => c.ProjectProposals).HasForeignKey(p => p.CreateBy).IsRequired();
            });
            modelBuilder.Entity<ProjectApprovalStep>(entity =>
            {
                entity.ToTable("ProjectApprovalStep");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.StepOrder).IsRequired();
                entity.Property(a => a.DecisionDate).IsRequired(false);
                entity.Property(a => a.Observations).IsRequired(false);
                entity.HasOne(p => p.ProjectProposal).WithMany(c => c.ProjectApprovalSteps).HasForeignKey(p => p.ProjectProposalId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApproverRole).WithMany(c => c.ProjectApprovalSteps).HasForeignKey(p => p.ApproverRoleId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApprovalStatus).WithMany(c => c.ProjectApprovalSteps).HasForeignKey(p => p.Status).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.User).WithMany(c => c.ProjectApprovalSteps).HasForeignKey(p => p.ApproverUserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
