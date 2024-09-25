//Commands.cs

/// <summary>
/// Command to check if an offer exists for a given GH_TUR.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="GhTur"></param>
/// <param name="DateTriggered"></param>
public record CheckOffer(Guid CorrelationId, string GhTur, DateTime DateTriggered);

/// <summary>
/// Command to create an upgrade offer.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="CheckResult"></param>
/// <param name="DateTriggered"></param>
public record CreateUpgradeOffer(Guid CorrelationId, CheckResult CheckResult, DateTime DateTriggered);

/// <summary>
/// Command to send an email notification.
/// </summary>
/// <param name="CorrelationId"></param>
/// <param name="Offer"></param>
public record SendEmailNotification(Guid CorrelationId, OfferRecord Offer);

/// <summary>
/// Simulated result of the offer check.
/// </summary>
/// <param name="IsLegacyOffer"></param>
/// <param name="OfferId"></param>
/// <param name="GhTur"></param>
public record CheckResult(bool IsLegacyOffer, Guid? OfferId, string GhTur);

public record OfferRecord(Guid Id, string GhTur, string CustomerName, string Creator, string FormNumber, DateTime CreateDate, Guid CreatedById);
