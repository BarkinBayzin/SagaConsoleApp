using MassTransit;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Messages
{
    public class OvercapacityRequestReceived : CorrelatedBy<Guid>
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
        public Guid CrmOpportunityId { get; set; }
        public string GhTur { get; set; }
    }

    public class OpportunityCreationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }

    public class UpgradeOfferCreated : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class UpgradeOfferCreationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string? Reason { get; set; }
    }
    public class SendNotificationEmail : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }
    public class NotificationEmailSent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class NotificationEmailFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
        public string? Reason { get; set; }
    }

    public class DeleteOffer : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class OfferDeleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class OfferDeletionFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }

    public class DeleteOpportunity : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OpportunityId { get; set; }
    }

    public class OpportunityDeleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OpportunityId { get; set; }
    }

    public class OpportunityDeletionFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }

    public class StartWorkflow : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class WorkflowApproved : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }

    public class WorkflowRejected : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }
    public class FinalizeWorkflow : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
    }


    public class UpdateOfferRequest : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
        public Guid OfferId { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
