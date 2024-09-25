using MassTransit;

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
                    await context.Publish(new EmailNotificationSent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        IsSuccess = false,
                        ErrorMessage = "E-posta gönderimi başarısız oldu"
                    });
                    return;
                }
                // Burada bilinçli bir hata oluşturuyoruz
                //throw new Exception("Simulated email sending error!");

                // Başarılı sonuç
                await context.Publish(new EmailNotificationSent
                {
                    CorrelationId = context.Message.CorrelationId,
                    IsSuccess = true,
                });
            }
            catch (Exception ex)
            {
                await context.Publish(new EmailNotificationSent 
                { 
                    CorrelationId = context.Message.CorrelationId, 
                    IsSuccess = false, 
                    ErrorMessage = "E-posta gönderimi başarısız oldu" 
                });
                throw;
            }
        }
    }
}
