using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Offer> Offers { get; set; }
        public DbSet<CrmOpportunity> CrmOpportunities { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Offer entity configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GhTur).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Creator).HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(1024);
            });

            // CrmOpportunity entity configuration
            modelBuilder.Entity<CrmOpportunity>(entity =>
            {
                entity.HasKey(e => e.OpportunityId);
                entity.Property(e => e.GhTur).IsRequired().HasMaxLength(256);
            });
        }
    }
}
