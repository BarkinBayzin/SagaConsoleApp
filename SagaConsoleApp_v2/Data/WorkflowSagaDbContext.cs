using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Saga;

namespace SagaConsoleApp_v2.Data
{
    public class WorkflowSagaDbContext : SagaDbContext
    {
        public WorkflowSagaDbContext(DbContextOptions<WorkflowSagaDbContext> options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new WorkflowSagaStateMap(); }
        }
    }

    public class WorkflowSagaStateMap : SagaClassMap<WorkflowSagaState>
    {
        protected override void Configure(EntityTypeBuilder<WorkflowSagaState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);
            entity.Property(x => x.OfferId);
            entity.Property(x => x.Created);
            entity.Property(x => x.Updated);
        }
    }

}
