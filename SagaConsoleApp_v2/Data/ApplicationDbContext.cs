using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Offer> Offers { get; set; }
        public DbSet<CrmOpportunity> CrmOpportunities { get; set; }
        public DbSet<OfferWorkflowHistory> OfferWorkflowHistories { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowTask> WorkflowTasks { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OfferWorkflowHistory>()
                .HasOne(a => a.Offer)
                .WithMany(a => a.OfferWorkflowHistories)
                .HasForeignKey(a => a.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfferWorkflowHistory>()
                .HasOne(a => a.WorkflowInstance)
                .WithOne(a => a.OfferWorkflowHistory)
                .HasForeignKey<OfferWorkflowHistory>(a => a.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
                        
            modelBuilder.Entity<OfferWorkflowHistory>()
                .OwnsMany(wr => wr.WorkflowReasons, navbuilder => navbuilder.ToJson());

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(w => w.OfferWorkflowHistory)
                .WithOne(o => o.WorkflowInstance)
                .HasForeignKey<WorkflowInstance>(w => w.Id);

            modelBuilder.Entity<WorkflowInstance>()
                .HasMany(w => w.WorkflowTasks)
                .WithOne(t => t.WorkflowInstance)
                .HasForeignKey(t => t.WorkflowInstanceId);

            modelBuilder.Entity<WorkflowTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TaskTitle).IsRequired(false).HasMaxLength(256);
                entity.Property(e => e.TaskDescription).HasMaxLength(1024);
                entity.Property(e => e.AssignedEmail).IsRequired(false).HasMaxLength(256);
                entity.Property(e => e.AssignedName).IsRequired(false).HasMaxLength(256);
            });

            modelBuilder.Entity<WorkflowInstance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StarterUserEmail).IsRequired(false).HasMaxLength(256);
                entity.Property(e => e.StarterFullName).IsRequired(false).HasMaxLength(256);
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GhTur).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Creator).HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(1024);
            });

            modelBuilder.Entity<CrmOpportunity>(entity =>
            {
                entity.HasKey(e => e.OpportunityId);
                entity.Property(e => e.GhTur).IsRequired().HasMaxLength(256);
            });
        }
    }
}
