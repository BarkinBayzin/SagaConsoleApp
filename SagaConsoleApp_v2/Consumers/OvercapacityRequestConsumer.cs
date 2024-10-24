using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class OvercapacityRequestConsumer : IConsumer<OvercapacityRequest>
    {
        private readonly IOfferService _offerService;
        private readonly ILogger<OvercapacityRequestConsumer> _logger;

        public OvercapacityRequestConsumer(IOfferService offerService, ILogger<OvercapacityRequestConsumer> logger)
        {
            _offerService = offerService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OvercapacityRequest> context)
        {
            _logger.LogInformation("[Consumer] [OvercapacityRequestConsumer] OvercapacityRequest alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            var checkResult = await _offerService.CheckGhTurAsync(context.Message.GhTur, checkOnlyInitial: true);

            if (checkResult.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [OvercapacityRequestConsumer] GhTur doğrulandı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                await context.Publish(new OvercapacityRequestAccepted
                {
                    CorrelationId = context.Message.CorrelationId,
                    GhTur = context.Message.GhTur
                });
            }
            else
            {
                _logger.LogWarning("[Consumer] [OvercapacityRequestConsumer] GhTur doğrulama başarısız, CorrelationId: {CorrelationId}, Reason: {Reason}", context.Message.CorrelationId, checkResult.Errors.FirstOrDefault());
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
