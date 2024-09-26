namespace SagaConsoleApp_v2.Entities
{
    public class AutomationProduct
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public AutomationUnit Unit { get; set; }
        public AutomationPhase Phase { get; set; }
    }
    public class AutomationUnit
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public class AutomationPhase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
