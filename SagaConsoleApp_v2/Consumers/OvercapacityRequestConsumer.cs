using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class OvercapacityRequestConsumer : IConsumer<OvercapacityRequest>
    {
        private readonly OfferService _offerService;

        public OvercapacityRequestConsumer(OfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<OvercapacityRequest> context)
        {
            var checkResult = await _offerService.CheckGhTurAsync(context.Message.GhTur, checkOnlyInitial: true);

            if (checkResult.IsSuccess)
            {
                await context.Publish(new OvercapacityRequestAccepted
                {
                    CorrelationId = context.Message.CorrelationId,
                    GhTur = context.Message.GhTur
                });
            }
            else
            {
                await context.Publish(new OvercapacityRequestRejected
                {
                    CorrelationId = context.Message.CorrelationId,
                    GhTur = context.Message.GhTur,
                    Reason = checkResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
