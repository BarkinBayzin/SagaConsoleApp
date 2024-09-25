using MassTransit;

namespace SagaConsoleApp.Saga
{
    public class OverCapacityUpgradeSagaState:SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string GhTur { get; set; }
        public DateTime DateTriggered { get; set; }
        public Guid? OfferId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
