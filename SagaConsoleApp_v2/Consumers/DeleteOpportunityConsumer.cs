using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class DeleteOpportunityConsumer : IConsumer<DeleteOpportunity>
    {
        private readonly CrmIntegrationService _crmIntegrationService;

        public DeleteOpportunityConsumer(CrmIntegrationService crmIntegrationService)
        {
            _crmIntegrationService = crmIntegrationService;
        }

        public async Task Consume(ConsumeContext<DeleteOpportunity> context)
        {
            try
            {
                var deleteResult = await _crmIntegrationService.DeleteOpportunityAsync(context.Message.OpportunityId);
                if (deleteResult.IsSuccess)
                {
                    await context.Publish(new OpportunityDeleted
                    {
                        CorrelationId = context.Message.CorrelationId,
                        OpportunityId = context.Message.OpportunityId
                    });
                }
                else
                {
                    await context.Publish(new OpportunityDeletionFailed
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Reason = deleteResult.Errors.FirstOrDefault()
                    });
                }
            }
            catch (Exception ex)
            {
                await context.Publish(new OpportunityDeletionFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = ex.Message
                });
            }
        }
    }
}
