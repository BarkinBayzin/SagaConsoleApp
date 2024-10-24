using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

public class SendNotificationEmailConsumer : IConsumer<SendNotificationEmail>
{
    private readonly EmailService _emailService;
    private readonly ILogger<SendNotificationEmailConsumer> _logger;

    /// <summary>
    /// Bildirim e-postası gönderen tüketici.
    /// </summary>
    public SendNotificationEmailConsumer(EmailService emailService, ILogger<SendNotificationEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendNotificationEmail> context)
    {
        _logger.LogInformation("[Consumer] [SendNotificationEmailConsumer] UpgradeOfferCreated alındı, CorrelationId: {CorrelationId}, UpgradeOfferId: {UpgradeOfferId}", context.Message.CorrelationId, context.Message.OfferId);

        try
        {
            var emailResult = await _emailService.SendOvercapacityNotificationAsync(context.Message.OfferId, null);

            if (emailResult.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [SendNotificationEmailConsumer] Bildirim e-postası gönderildi, CorrelationId: {CorrelationId}, UpgradeOfferId: {UpgradeOfferId}", context.Message.CorrelationId, context.Message.OfferId);
                await context.Publish(new NotificationEmailSent
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = context.Message.OfferId
                });
            }
            else
            {
                _logger.LogWarning("[Consumer] [SendNotificationEmailConsumer] Bildirim e-postası gönderilemedi, CorrelationId: {CorrelationId}, UpgradeOfferId: {UpgradeOfferId}, Reason: {Reason}", context.Message.CorrelationId, context.Message.OfferId, emailResult.Errors.FirstOrDefault());
                await context.Publish(new NotificationEmailFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = context.Message.OfferId,
                    Reason = emailResult.Errors.FirstOrDefault()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Consumer] [SendNotificationEmailConsumer] Bildirim e-postası gönderilirken hata oluştu, CorrelationId: {CorrelationId}, UpgradeOfferId: {UpgradeOfferId}, Hata: {ErrorMessage}", context.Message.CorrelationId, context.Message.OfferId, ex.Message);
            await context.Publish(new NotificationEmailFailed
            {
                CorrelationId = context.Message.CorrelationId,
                OfferId = context.Message.OfferId,
                Reason = ex.Message
            });
        }
    }
}
