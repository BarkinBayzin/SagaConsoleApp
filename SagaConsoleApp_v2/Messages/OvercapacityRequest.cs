﻿using MassTransit;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Messages
{
    public class OvercapacityRequest : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
        public DateTime DateTriggered { get; set; }
        public IEnumerable<AutomationProduct> Products { get; set; }
    }
    public class OvercapacityRequestAccepted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
    }

    public class OvercapacityRequestRejected : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
        public string? Reason { get; set; }
    }

    public class OpportunityCreated : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public CrmOpportunity Opportunity { get; set; }
    }

    public class OpportunityCreationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }

    public class UpgradeOfferCreated : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Offer UpgradeOffer { get; set; }
    }

    public class UpgradeOfferCreationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string? Reason { get; set; }
    }

    public class NotificationEmailSent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }

    public class NotificationEmailFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string? Reason { get; set; }
    }
}
