using MassTransit;
using SagaConsoleApp.Messages;

namespace SagaConsoleApp.Consumers
{
    public class CheckOfferConsumer : IConsumer<CheckOffer>
    {
        public async Task Consume(ConsumeContext<CheckOffer> context)
        {
            Console.WriteLine($"[Consumer] Teklif Kontrol Ediliyor: GhTur={context.Message.GhTur}");

            if (context.Message.GhTur == "FAIL_CHECK_OFFER")
            {
                await context.Publish(new OfferChecked(context.Message.CorrelationId, false, "Teklif kontrolü başarısız oldu", null));
                return;
            }

            // Başarılı sonuç
            var result = new CheckResult(false, Guid.NewGuid(), context.Message.GhTur);
            await context.Publish(new OfferChecked(context.Message.CorrelationId, true, null, result));
        }
    }
}
