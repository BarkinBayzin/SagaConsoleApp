using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    public class StartWorkflowConsumer : IConsumer<NotificationEmailSent>
    {
        private readonly ILogger<StartWorkflowConsumer> _logger;
        private readonly IOfferService _offerService;

        public StartWorkflowConsumer(ILogger<StartWorkflowConsumer> logger, IOfferService offerService)
        {
            _logger = logger;
            _offerService = offerService;
        }

        public async Task Consume(ConsumeContext<NotificationEmailSent> context)
        {
            _logger.LogInformation("[Consumer] [StartWorkflowConsumer] StartWorkflow alındı, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

            // Workflow işlemlerini başlatın
            // Örneğin, otomatik onaylama işlemi yapabilirsiniz

            // Otomatik onaylama işlemi
            //await context.Publish(new WorkflowApproved
            //{
            //    CorrelationId = context.Message.CorrelationId,
            //    OfferId = context.Message.OfferId
            //});
        }
    }
    public class StartWorkflowConsumerDefinition : ConsumerDefinition<StartWorkflowConsumer>
    {
        public StartWorkflowConsumerDefinition()
        {
            EndpointName = "start-workflow";
        }
    }
}
