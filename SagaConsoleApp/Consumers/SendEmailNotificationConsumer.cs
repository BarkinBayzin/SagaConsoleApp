using MassTransit;
using SagaConsoleApp.Messages;

namespace SagaConsoleApp.Consumers
{
    public class SendEmailNotificationConsumer : IConsumer<SendEmailNotification>
    {
        public async Task Consume(ConsumeContext<SendEmailNotification> context)
        {
            Console.WriteLine($"[Consumer] E-posta Bildirimi Gönderiliyor: OfferId={context.Message.Offer.Id}");

            try
            {
                if (context.Message.Offer.GhTur == "FAIL_SEND_EMAIL")
                {
                    await context.Publish(new EmailNotificationSent(context.Message.CorrelationId, false, "E-posta gönderimi başarısız oldu"));
                    return;
                }
                // Burada bilinçli bir hata oluşturuyoruz
                //throw new Exception("Simulated email sending error!");

                // Başarılı sonuç
                await context.Publish(new EmailNotificationSent(context.Message.CorrelationId, true, null));
            }
            catch (Exception ex)
            {
                await context.Publish(new EmailNotificationSent(context.Message.CorrelationId, false, "E-posta gönderimi başarısız oldu Sistem Mesajı => " + ex.Message));

                throw;
            }
        }
    }
}
