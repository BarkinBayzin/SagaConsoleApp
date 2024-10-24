using SagaConsoleApp_v2.Entities.Enums;

namespace SagaConsoleApp_v2.Entities
{
    public class Offer
    {
        public Guid Id { get; private set; }
        public string GhTur { get; private set; }
        public DateTime CreateDate { get; private set; }
        public string Creator { get; private set; }
        public string Description { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDeleted { get; private set; }
        public WorkflowTaskStatus Status { get; set; }
        public List<OfferWorkflowHistory> OfferWorkflowHistories { get; set; }
        public static Offer CreateUpgrade(
            string creator,
            string ghTur,
            DateTime createDate,
            string description)
        {
            return new Offer
            {
                Creator = creator,
                GhTur = ghTur,
                CreateDate = createDate,
                Description = description,
                IsDraft = true,
                IsDeleted = false
            };
        }

        public void MarkAsDelete()
        {
            IsDeleted = true;
        }

        public void RevertDelete()
        {
            IsDeleted = false;
        }

        public void AuditCreated(DateTime date, string userId, string userEmail, string userFullName)
        {
            // Denetleme işlemleri...
        }

        public Offer() { }
    }
}
