using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Saga;
using System.Text.Json;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Data
{
    public class SagaStateDbContext : SagaDbContext
    {
        public SagaStateDbContext(DbContextOptions<SagaStateDbContext> options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OvercapacitySagaStateMap(); }
        }
    }

    public class OvercapacitySagaStateMap : SagaClassMap<OvercapacitySagaState>
    {
        protected override void Configure(EntityTypeBuilder<OvercapacitySagaState> builder, ModelBuilder model)
        {
            builder.ToTable("OvercapacitySagaStates");
            builder.HasKey(x => x.CorrelationId);

            builder.Property(x => x.GhTur).HasMaxLength(256).IsRequired();
            builder.Property(x => x.CurrentState).HasMaxLength(1024);
            builder.Property(x => x.FailureReason).HasMaxLength(4000);
            builder.Property(x => x.FailureReason).HasMaxLength(4000).IsRequired(false);
        }
    }
}
