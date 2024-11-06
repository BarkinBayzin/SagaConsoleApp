using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class CreateOpportunityConsumer : IConsumer<CreateOpportunity>
    {
        private readonly CrmIntegrationService _crmIntegrationService;
        private readonly ILogger<CreateOpportunityConsumer> _logger;

        public CreateOpportunityConsumer(CrmIntegrationService crmIntegrationService, ILogger<CreateOpportunityConsumer> logger)
        {
            _crmIntegrationService = crmIntegrationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateOpportunity> context)
        {
            _logger.LogInformation("[Consumer] [CreateOpportunityConsumer] CreateOpportunity komutu alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            var existingOpportunity = await _crmIntegrationService.CheckExistingOpportunityAsync(context.Message.GhTur);
            if (existingOpportunity.IsSuccess)
            {
                _logger.LogWarning("[Consumer] Opportunity already exists, skipping. CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                // Eğer fırsat zaten varsa, doğrudan OpportunityCreated olayı yayınlayabilirsiniz
                await context.Publish(new OpportunityCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    CrmOpportunityId = existingOpportunity.Value.OpportunityId,
                    GhTur = context.Message.GhTur
                });
                return;
            }

            var crmResult = await _crmIntegrationService.CreateUpgradeOpportunityAsync(context.Message.GhTur, DateTime.UtcNow);

            if (crmResult.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [CreateOpportunityConsumer] CRM Opportunity oluşturuldu, CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                await context.Publish(new OpportunityCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    CrmOpportunityId = crmResult.Value.OpportunityId,
                    GhTur = crmResult.Value.GhTur
                });
            }
            else
            {
                _logger.LogError("[Consumer] [CreateOpportunityConsumer] CRM Opportunity oluşturulamadı, CorrelationId: {CorrelationId}, Reason: {Reason}", context.Message.CorrelationId, crmResult.Errors.FirstOrDefault());
                await context.Publish(new OpportunityCreationFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = crmResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
