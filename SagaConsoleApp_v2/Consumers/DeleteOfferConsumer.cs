using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

public class DeleteOfferConsumer : IConsumer<DeleteOffer>
{
    private readonly IOfferService _offerService;
    private readonly ILogger<DeleteOfferConsumer> _logger;

    public DeleteOfferConsumer(IOfferService offerService, ILogger<DeleteOfferConsumer> logger)
    {
        _offerService = offerService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteOffer> context)
    {
        _logger.LogInformation("[Consumer] [DeleteOfferConsumer] DeleteOffer alındı, CorrelationId: {CorrelationId}, OfferId: {OfferId}", context.Message.CorrelationId, context.Message.OfferId);

        try
        {
            var result = await _offerService.DeleteOfferAsync(context.Message.OfferId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[Consumer] [DeleteOfferConsumer] Offer silme işlemi başarılı, CorrelationId: {CorrelationId}, OfferId: {OfferId}", context.Message.CorrelationId, context.Message.OfferId);
                await context.Publish(new OfferDeleted
                {
                    CorrelationId = context.Message.CorrelationId,
                    OfferId = context.Message.OfferId
                });
            }
            else
            {
                _logger.LogWarning("[Consumer] [DeleteOfferConsumer] Offer silme işlemi başarısız, CorrelationId: {CorrelationId}, OfferId: {OfferId}, Reason: {Reason}", context.Message.CorrelationId, context.Message.OfferId, result.Errors.FirstOrDefault());
                await context.Publish(new OfferDeletionFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = result.Errors.FirstOrDefault()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Consumer] [DeleteOfferConsumer] Offer silme sırasında hata oluştu, CorrelationId: {CorrelationId}, OfferId: {OfferId}, Hata: {ErrorMessage}", context.Message.CorrelationId, context.Message.OfferId, ex.Message);
            await context.Publish(new OfferDeletionFailed
            {
                CorrelationId = context.Message.CorrelationId,
                Reason = ex.Message
            });
        }
    }
}
