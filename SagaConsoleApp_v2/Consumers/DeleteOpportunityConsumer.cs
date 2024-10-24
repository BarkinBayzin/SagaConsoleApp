using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

public class DeleteOpportunityConsumer : IConsumer<DeleteOpportunity>
{
    private readonly CrmIntegrationService _crmIntegrationService;
    private readonly ILogger<DeleteOpportunityConsumer> _logger;

    public DeleteOpportunityConsumer(CrmIntegrationService crmIntegrationService, ILogger<DeleteOpportunityConsumer> logger)
    {
        _crmIntegrationService = crmIntegrationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteOpportunity> context)
    {
        _logger.LogInformation("[Consumer] [DeleteOpportunityConsumer] DeleteOpportunity alındı, CorrelationId: {CorrelationId}, OpportunityId: {OpportunityId}", context.Message.CorrelationId, context.Message.OpportunityId);

        try
        {
            var deleteResult = await _crmIntegrationService.DeleteOpportunityAsync(context.Message.OpportunityId);

            if (deleteResult.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [DeleteOpportunityConsumer] Opportunity silme işlemi başarılı, CorrelationId: {CorrelationId}, OpportunityId: {OpportunityId}", context.Message.CorrelationId, context.Message.OpportunityId);
                await context.Publish(new OpportunityDeleted
                {
                    CorrelationId = context.Message.CorrelationId,
                    OpportunityId = context.Message.OpportunityId
                });
            }
            else
            {
                _logger.LogWarning("[Consumer] [DeleteOpportunityConsumer] Opportunity silme işlemi başarısız, CorrelationId: {CorrelationId}, OpportunityId: {OpportunityId}, Reason: {Reason}", context.Message.CorrelationId, context.Message.OpportunityId, deleteResult.Errors.FirstOrDefault());
                await context.Publish(new OpportunityDeletionFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = deleteResult.Errors.FirstOrDefault()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Consumer] [DeleteOpportunityConsumer] Opportunity silme sırasında hata oluştu, CorrelationId: {CorrelationId}, OpportunityId: {OpportunityId}, Hata: {ErrorMessage}", context.Message.CorrelationId, context.Message.OpportunityId, ex.Message);
            await context.Publish(new OpportunityDeletionFailed
            {
                CorrelationId = context.Message.CorrelationId,
                Reason = ex.Message
            });
        }
    }
}
