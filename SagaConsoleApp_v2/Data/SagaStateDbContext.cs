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
        protected override void Configure(EntityTypeBuilder<OvercapacitySagaState> entity, ModelBuilder model)
        {
            entity.ToTable("OvercapacitySagaStates");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);

            // Diğer özelliklerin eşleştirilmesi
            entity.Property(x => x.GhTur).HasMaxLength(256);
            entity.Property(x => x.FailureReason).HasMaxLength(1024);

            // Complex types için ek yapılandırmalar
            // Örneğin, CrmOpportunity ve UpgradeOffer JSON olarak saklanabilir
            entity.Property(x => x.CrmOpportunity).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<CrmOpportunity>(v, (JsonSerializerOptions)null!)!);
        }
    }
}
