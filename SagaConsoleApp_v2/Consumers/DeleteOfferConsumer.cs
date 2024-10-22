using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class DeleteOfferConsumer : IConsumer<DeleteOffer>
    {
        private readonly OfferService _offerService;

        public DeleteOfferConsumer(OfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<DeleteOffer> context)
        {
            try
            {
                await _offerService.DeleteOfferAsync(context.Message.OfferId);
                await context.Publish(new OfferDeleted
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = context.Message.OfferId
                });
            }
            catch (Exception ex)
            {
                await context.Publish(new OfferDeletionFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = ex.Message
                });
            }
        }
    }
}
