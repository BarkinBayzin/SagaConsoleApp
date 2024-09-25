namespace SagaConsoleApp.Messages
{
    public record OverCapacityRequestReceived(Guid CorrelationId, string GhTur, DateTime DateTriggered);

    public record CheckOffer(Guid CorrelationId, string GhTur, DateTime DateTriggered);

    public record OfferChecked(Guid CorrelationId, bool IsSuccess, string? ErrorMessage, CheckResult Result);

    public record CreateUpgradeOffer(Guid CorrelationId, CheckResult CheckResult, DateTime DateTriggered);

    public record UpgradeOfferCreated(Guid CorrelationId, bool IsSuccess, string? ErrorMessage, OfferDto Offer);

    public record SendEmailNotification(Guid CorrelationId, OfferDto Offer);

    public record EmailNotificationSent(Guid CorrelationId, bool IsSuccess, string? ErrorMessage);

    public record CheckResult(bool IsLegacyOffer, Guid? OfferId, string GhTur);

    public record OfferDto(Guid Id, string GhTur, string CustomerName, string Creator, string FormNumber, DateTime CreateDate, Guid CreatedById);
    public record RollbackUpgradeOffer(Guid CorrelationId); 
}
