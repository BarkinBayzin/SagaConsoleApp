using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace SagaConsoleApp.Saga
{
    public class OverCapacityUpgradeSagaMap : SagaClassMap<OverCapacityUpgradeSagaState>
    {
        protected override void Configure(EntityTypeBuilder<OverCapacityUpgradeSagaState> entity, ModelBuilder model)
        {
            entity.ToTable("OverCapacityUpgradeSagaStates");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState);
            entity.Property(x => x.GhTur);
            entity.Property(x => x.DateTriggered);
            entity.Property(x => x.OfferId);
            entity.Property(x => x.ErrorMessage).IsRequired(false);
        }
    }
}
