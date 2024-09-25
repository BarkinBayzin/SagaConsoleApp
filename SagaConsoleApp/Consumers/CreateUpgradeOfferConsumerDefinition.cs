// Consumers/CreateUpgradeOfferConsumerDefinition.cs
using MassTransit;
namespace SagaConsoleApp.Consumers
{
    public class CreateUpgradeOfferConsumerDefinition : ConsumerDefinition<CreateUpgradeOfferConsumer>
    {
        public CreateUpgradeOfferConsumerDefinition()
        {
            EndpointName = "create-upgrade-offer";
        }
    }
}
