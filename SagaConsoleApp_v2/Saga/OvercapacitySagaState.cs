using MassTransit;

namespace SagaConsoleApp_v2.Saga
{
    public class OvercapacitySagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
        public Guid CrmOpportunityId { get; set; }
        public Guid InitialOfferId { get; set; }
        public string CurrentState { get; set; }
        public string? FailureReason { get; set; }
        public bool IsCompleted { get; set; }
        public Guid OfferId { get; set; }
    }

}
