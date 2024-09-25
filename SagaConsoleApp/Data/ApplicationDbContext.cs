using Microsoft.EntityFrameworkCore;
using SagaConsoleApp.Models;

namespace SagaConsoleApp.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Offer>().HasKey(x => x.Id);
        }
    }
}
