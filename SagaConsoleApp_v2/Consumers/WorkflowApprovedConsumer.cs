using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class WorkflowApprovedConsumer : IConsumer<WorkflowApproved>
    {
        private readonly ILogger<WorkflowApprovedConsumer> _logger;
        private readonly IOfferService _offerService;
        public WorkflowApprovedConsumer(ILogger<WorkflowApprovedConsumer> logger, IOfferService offerService)
        {
            _logger = logger;
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<WorkflowApproved> context)
        {
            _logger.LogInformation("[Consumer] [WorkflowApprovedConsumer] WorkflowApproved alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            // Teklifin durumunu güncelle
            // CRM'e submit işlemi yap
            // Hata olursa telafi işlemlerini başlat

            // Örnek olarak, hata fırlatalım
            try
            {
                // CRM'e submit işlemi
                // Eğer hata olursa exception fırlat
                await _offerService.ApproveOfferAsync(context.Message.OfferId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Consumer] [WorkflowApprovedConsumer] Hata oluştu, CorrelationId: {CorrelationId}", context.Message.CorrelationId);
            }
        }
    }

}
