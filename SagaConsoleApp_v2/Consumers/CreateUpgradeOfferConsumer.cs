using MassTransit;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class CreateUpgradeOfferConsumer : IConsumer<OpportunityCreated>
    {
        private readonly IOfferService _offerService;
        private readonly ILogger<CreateUpgradeOfferConsumer> _logger;

        /// <summary>
        /// Upgrade teklifi oluşturan tüketici.
        /// </summary>
        public CreateUpgradeOfferConsumer(IOfferService offerService, ILogger<CreateUpgradeOfferConsumer> logger)
        {
            _offerService = offerService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OpportunityCreated> context)
        {
            _logger.LogInformation("[Consumer] [CreateUpgradeOfferConsumer] OpportunityCreated alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            var request = new OvercapacityRequest
            {
                GhTur = context.Message.Opportunity.GhTur,
                DateTriggered = DateTime.UtcNow,
                Products = new List<AutomationProduct>() // Ürün listesi
            };

            var offerResult = await _offerService.CreateUpgradeOfferAsync(Guid.NewGuid(), request, context.Message.Opportunity);

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