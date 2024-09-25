// Consumers/CheckOfferConsumerDefinition.cs
using MassTransit;

public class CheckOfferConsumerDefinition : ConsumerDefinition<CheckOfferConsumer>
{
    public CheckOfferConsumerDefinition()
    {
        EndpointName = "check-offer";
    }
}
