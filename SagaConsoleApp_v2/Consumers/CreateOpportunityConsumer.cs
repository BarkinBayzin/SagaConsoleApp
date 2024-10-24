﻿using MassTransit;
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
        private readonly ILogger<CreateOpportunityConsumer> _logger;

        public CreateOpportunityConsumer(CrmIntegrationService crmIntegrationService, ILogger<CreateOpportunityConsumer> logger)
        {
            _crmIntegrationService = crmIntegrationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OvercapacityRequestAccepted> context)
        {
            _logger.LogInformation("[Consumer] [CreateOpportunityConsumer] OvercapacityRequestAccepted alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

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
