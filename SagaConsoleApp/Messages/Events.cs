//Events.cs

/// <summary>
/// Initial event that is triggered when the system detects that the capacity is over the limit.
/// </summary>
public class OverCapacityRequestReceived
{
    public Guid CorrelationId { get; set; }
    public string GhTur { get; set; }
    public DateTime DateTriggered { get; set; }
}
/// <summary>
/// Event that is triggered when the offer is checked.
/// </summary>
public class OfferChecked
{
    public Guid CorrelationId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public CheckResult Result { get; set; }
}
/// <summary>
/// Event that is triggered when the upgrade offer is created.
/// </summary>
public class UpgradeOfferCreated
{
    public Guid CorrelationId { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public OfferRecord Offer { get; set; }
}

/// <summary>
/// Event that is triggered when the email notification is sent.
/// </summary>
public class EmailNotificationSent
{
    public Guid CorrelationId { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Compansation event that is triggered when the upgrade offer is rolled back.
/// </summary>
public class CompensationRequest
{
    public Guid CorrelationId { get; set; }
    public string Reason { get; set; }
}
