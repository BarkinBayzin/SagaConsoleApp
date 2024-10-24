using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class WorkflowRejectedConsumer : IConsumer<WorkflowRejected>
    {
        private readonly ILogger<WorkflowRejectedConsumer> _logger;
        private readonly IOfferService _offerService;

        public WorkflowRejectedConsumer(ILogger<WorkflowRejectedConsumer> logger, IOfferService offerService)
        {
            _logger = logger;
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<WorkflowRejected> context)
        {
            _logger.LogInformation("[Consumer] [WorkflowRejectedConsumer] WorkflowRejected alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            try
            {
                // Teklifin durumunu güncelle
                await _offerService.RejectOfferAsync(context.Message.OfferId);

                // Gerekirse başka işlemler yapın veya olaylar yayınlayın
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Consumer] [WorkflowRejectedConsumer] Hata oluştu, CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                // Telafi işlemleri için gerekirse bir olay yayınlayın
            }
        }
    }


}
