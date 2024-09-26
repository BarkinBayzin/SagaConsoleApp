using MassTransit;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    /// <summary>
    /// Upgrade teklifi oluşturan tüketici.
    /// </summary>
    public class CreateUpgradeOfferConsumer : IConsumer<OpportunityCreated>
    {
        private readonly OfferService _offerService;

        public CreateUpgradeOfferConsumer(OfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<OpportunityCreated> context)
        {
            var request = new OvercapacityRequest
            {
                GhTur = context.Message.Opportunity.GhTur,
                DateTriggered = DateTime.UtcNow,
                Products = new List<AutomationProduct>() // Ürün listesi
            };

            var offerResult = await _offerService.CreateUpgradeOfferAsync(Guid.NewGuid(), request, context.Message.Opportunity);

            if (offerResult.IsSuccess)
            {
                await context.Publish(new UpgradeOfferCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    UpgradeOffer = offerResult.Value
                });
            }
            else
            {
                await context.Publish(new UpgradeOfferCreationFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = offerResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
