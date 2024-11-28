using Ardalis.Result;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;

namespace SagaConsoleApp_v2.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<Result> SendOvercapacityNotificationAsync(Guid offerId, OvercapacityRequestReceived? request)
        {
            try
            {
                _logger.LogInformation("[EmailService] [SendOvercapacityNotificationAsync] Overcapacity bildirim e-postası gönderiliyor, OfferId: {OfferId}", offerId);
                //throw new Exception();
                // E-posta gönderme işlemi simülasyonu
                //await Task.Delay(1000);

                _logger.LogInformation("[EmailService] [SendOvercapacityNotificationAsync] E-posta başarıyla gönderildi, OfferId: {OfferId}", offerId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] [SendOvercapacityNotificationAsync] E-posta gönderilirken hata oluştu, Hata mesajı: {Message}", ex.Message);
                return Result.Error($"E-posta gönderilirken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result> SendNotificationEmailAsync(Offer offer) 
        {
            try
            {
                _logger.LogInformation("[EmailService] [SendNotificationEmailAsync] Overcapacity bildirim e-postası gönderiliyor, OfferId: {OfferId}", offer.Id);
                //throw new Exception();
                // E-posta gönderme işlemi simülasyonu
                //await Task.Delay(1000);
                _logger.LogInformation("[EmailService] [SendNotificationEmailAsync] E-posta başarıyla gönderildi, OfferId: {OfferId}", offer.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] [SendNotificationEmailAsync] E-posta gönderilirken hata oluştu, Hata mesajı: {Message}", ex.Message);
                return Result.Error($"E-posta gönderilirken hata oluştu: {ex.Message}");
            }
        }
    }
}
