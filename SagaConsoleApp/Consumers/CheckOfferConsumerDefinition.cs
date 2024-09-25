using MassTransit;

namespace SagaConsoleApp.Consumers
{
    public class CheckOfferConsumerDefinition : ConsumerDefinition<CheckOfferConsumer>
    {
        public CheckOfferConsumerDefinition()
        {
            EndpointName = "check-offer";
        }
    }
}
