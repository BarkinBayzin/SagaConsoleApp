// Consumers/CreateUpgradeOfferConsumerDefinition.cs
using MassTransit;

public class CreateUpgradeOfferConsumerDefinition : ConsumerDefinition<CreateUpgradeOfferConsumer>
{
    public CreateUpgradeOfferConsumerDefinition()
    {
        EndpointName = "create-upgrade-offer";
    }
}
