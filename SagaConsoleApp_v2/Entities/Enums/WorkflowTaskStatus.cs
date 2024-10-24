namespace SagaConsoleApp_v2.Entities.Enums
{
    public enum WorkflowTaskStatus
    {
        Waiting = 0,
        Assigned = 10, //sarı -- dashboard için waitingtir
        Approved = 20, //yeşil
        Rejected = 30,
        Canceled = 40,

        Exception = 400 //PMExceptiona gittiğini gösterir. PMExceptiona giden tasklar için bu status kullanılır.

    }
}
