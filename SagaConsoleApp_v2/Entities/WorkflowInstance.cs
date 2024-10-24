using SagaConsoleApp_v2.Entities.Enums;

namespace SagaConsoleApp_v2.Entities
{
    public class WorkflowInstance
    {
        public Guid Id { get; set; }
        public WorkflowInstanceStatus Status { get; private set; }
        public string StarterFullName { get; private set; }
        public string StarterUserEmail { get; private set; }
        public string StarterUserId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public OfferWorkflowHistory OfferWorkflowHistory { get; private set; }
        public ICollection<WorkflowTask> WorkflowTasks { get; private set; }

    }
}
