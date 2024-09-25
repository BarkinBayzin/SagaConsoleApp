using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp.Saga;

public class SagaStateDbContext : SagaDbContext
{
    public SagaStateDbContext(DbContextOptions<SagaStateDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OverCapacityUpgradeSagaState>(entity => entity.HasKey(entity => entity.CorrelationId));
        modelBuilder.Entity<OverCapacityUpgradeSagaState>(entity =>
        {
            entity.Property(e => e.ErrorMessage).IsRequired(false);
        });
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get
        {
            yield return new OverCapacityUpgradeSagaMap();
        }
    }
}
