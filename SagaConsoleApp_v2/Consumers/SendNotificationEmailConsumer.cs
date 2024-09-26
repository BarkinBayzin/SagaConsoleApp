using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Consumers
{
    /// <summary>
    /// Bildirim e-postası gönderen tüketici.
    /// </summary>
    public class SendNotificationEmailConsumer : IConsumer<UpgradeOfferCreated>
    {
        private readonly EmailService _emailService;

        public SendNotificationEmailConsumer(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<UpgradeOfferCreated> context)
        {
            var emailResult = await _emailService.SendOvercapacityNotificationAsync(context.Message.UpgradeOffer, null);

            if (emailResult.IsSuccess)
            {
                await context.Publish(new NotificationEmailSent
                {
                    CorrelationId = context.Message.CorrelationId
                });
            }
            else
            {
                await context.Publish(new NotificationEmailFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = emailResult.Errors.FirstOrDefault()
                });
            }
        }
    }
}
