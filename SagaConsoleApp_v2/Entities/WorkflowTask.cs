using SagaConsoleApp_v2.Entities.Enums;

namespace SagaConsoleApp_v2.Entities
{
    public class WorkflowTask
    {
        public Guid Id { get; private set; }
        public Guid WorkflowInstanceId { get; private set; }
        public WorkflowInstance WorkflowInstance { get; private set; }
        public StateType State { get; private set; }
        public string TaskTitle { get; private set; }
        public string TaskDescription { get; private set; }
        public AssignedUserType AssignedType { get; private set; }
        public Guid AssignedId { get; private set; }
        public string AssignedName { get; private set; }
        public string AssignedEmail { get; private set; }
        public DateTime AssignDate { get; private set; }
        public WorkflowTaskStatus TaskStatus { get; private set; }
        public DateTime? CompleteDate { get; private set; }
    }

}
