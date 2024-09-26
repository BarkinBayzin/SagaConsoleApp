using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    /// <summary>
    /// CRM'de fırsat oluşturan tüketici.
    /// </summary>
    public class CreateOpportunityConsumer : IConsumer<OvercapacityRequestAccepted>
    {
        private readonly CrmIntegrationService _crmIntegrationService;

        public CreateOpportunityConsumer(CrmIntegrationService crmIntegrationService)
        {
            _crmIntegrationService = crmIntegrationService;
        }

        public async Task Consume(ConsumeContext<OvercapacityRequestAccepted> context)
        {
            var crmResult = await _crmIntegrationService.CreateUpgradeOpportunityAsync(context.Message.GhTur, DateTime.UtcNow);

            if (crmResult.IsSuccess)
            {
                await context.Publish(new OpportunityCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    Opportunity = crmResult.Value
                });
            }
            else
            {
                await context.Publish(new OpportunityCreationFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = crmResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
