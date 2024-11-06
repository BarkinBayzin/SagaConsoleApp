using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class CreateUpgradeOfferConsumer : IConsumer<OpportunityCreated>
    {
        private readonly IOfferService _offerService;
        private readonly ILogger<CreateUpgradeOfferConsumer> _logger;

        public CreateUpgradeOfferConsumer(IOfferService offerService, ILogger<CreateUpgradeOfferConsumer> logger)
        {
            _offerService = offerService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OpportunityCreated> context)
        {
            _logger.LogInformation("[Consumer] [CreateUpgradeOfferConsumer] CreateUpgradeOffer alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            // Zaten upgrade teklifi oluşturulmuş mu kontrol et
            var existingOffer = await _offerService.CheckExistingOfferAsync(context.Message.GhTur, context.Message.CrmOpportunityId);
            if (existingOffer.IsSuccess)
            {
                _logger.LogWarning("[Consumer] Offer already exists, skipping. CorrelationId: {CorrelationId}", context.Message.CorrelationId);

                // Zaten varsa UpgradeOfferCreated eventini yayınla
                await context.Publish(new UpgradeOfferCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = existingOffer.Value.Id
                });

                return;
            }

            var offerResult = await _offerService.CreateUpgradeOfferAsync(Guid.NewGuid(), context.Message.GhTur, context.Message.CrmOpportunityId);

            if (offerResult.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [CreateUpgradeOfferConsumer] Upgrade Offer oluşturuldu, CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                await context.Publish(new UpgradeOfferCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = offerResult.Value.Id
                });
            }
            else
            {
                _logger.LogError("[Consumer] [CreateUpgradeOfferConsumer] Upgrade Offer oluşturulamadı, CorrelationId: {CorrelationId}, Reason: {Reason}", context.Message.CorrelationId, offerResult.Errors.FirstOrDefault());
                await context.Publish(new UpgradeOfferCreationFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = offerResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
