using MassTransit;

namespace SagaConsoleApp.Consumers
{
    public class SendEmailNotificationConsumerDefinition:ConsumerDefinition<SendEmailNotificationConsumer>
    {
        public SendEmailNotificationConsumerDefinition()
        {
            EndpointName = "send-email-notification";
        }
    }
}
