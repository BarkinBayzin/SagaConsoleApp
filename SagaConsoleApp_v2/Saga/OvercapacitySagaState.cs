using MassTransit;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Saga
{
    public class OvercapacitySagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public string GhTur { get; set; }
        public CrmOpportunity CrmOpportunity { get; set; }
        public Offer UpgradeOffer { get; set; }
        public string FailureReason { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
