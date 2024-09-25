// Consumers/CheckOfferConsumer.cs
using MassTransit;

namespace SagaConsoleApp.Consumers
{
    public class CheckOfferConsumer : IConsumer<CheckOffer>
    {
        public async Task Consume(ConsumeContext<CheckOffer> context)
        {
            Console.WriteLine($"[Consumer] Checking offer for GhTur={context.Message.GhTur}");

            // Simulate checking offer
            await Task.Delay(500);

            var result = new CheckResult(false, Guid.NewGuid(), context.Message.GhTur);

            // Simulate success
            await context.Publish(new OfferChecked
            {
                CorrelationId = context.Message.CorrelationId,
                IsSuccess = true,
                Result = result
            });
        }
    }
}