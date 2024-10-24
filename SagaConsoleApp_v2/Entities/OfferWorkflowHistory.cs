using SagaConsoleApp_v2.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SagaConsoleApp_v2.Entities
{
    public class OfferWorkflowHistory
    {
        protected OfferWorkflowHistory() { }

        public OfferWorkflowHistory(Offer offer,
            ApproverUser? technical,
            ApproverUser? manager,
            ApproverUser? cm,
            ApproverUser? cfo,
            ApproverUser? ceo,
            ApproverUser? am,
            ApproverUser? cto,
            List<WorkflowReason>? workflowReasons,
            WorkflowType workflowType
            )
        {
            this.OfferId = offer.Id;
            this.Offer = offer;

            // Diğer atamalar...

            _workflowReasons = workflowReasons;
            this.WorkflowType = workflowType;
        }

        public Guid Id { get; private set; }
        public Guid OfferId { get; private set; }
        public Offer Offer { get; private set; }
        public Guid? WorkflowInstanceId { get; private set; }
        public WorkflowInstance WorkflowInstance { get; private set; }
        public WorkflowType WorkflowType { get; private set; }

        // Diğer özellikler...

        #region WorkflowReason
        private List<WorkflowReason>? _workflowReasons = null;
        public IReadOnlyCollection<WorkflowReason> WorkflowReasons => _workflowReasons?.AsReadOnly();
        #endregion

        public void AddWorkflowReason(WorkflowReason reason)
        {
            if (_workflowReasons == null)
            {
                _workflowReasons = new List<WorkflowReason>();
            }

            _workflowReasons.Add(reason);
        }

        public void Approve(Guid taskOwnerId, string? reason, StateType stateType)
        {
            AddWorkflowReason(new WorkflowReason
            {
                TaskOwnerId = taskOwnerId,
                Reason = reason,
                StateType = stateType
            });
        }

        public void Reject(Guid taskOwnerId, string? reason, StateType stateType)
        {
            AddWorkflowReason(new WorkflowReason
            {
                TaskOwnerId = taskOwnerId,
                Reason = reason,
                StateType = stateType
            });
        }

        public void SetWorkflowInstanceId(Guid workflowInstanceId)
        {
            WorkflowInstanceId = workflowInstanceId;
        }

        public void SetWorkflowInstance(WorkflowInstance workflowInstance)
        {
            WorkflowInstance = workflowInstance;
        }
        

    }

    public struct ApproverUser
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public Guid? UserId { get; set; }
    }

    public class WorkflowReason
    {
        [JsonPropertyName("taskOwnerId")]
        public Guid TaskOwnerId { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("stateType")]
        public StateType StateType { get; set; } = StateType.None;
    }

    public enum WorkflowType
    {
        [Display(Name = "Offer Approval")]
        OfferApproval = 10,
        [Display(Name = "Offer Leasing Approval")]
        OfferLeasing = 20,
    }
}
