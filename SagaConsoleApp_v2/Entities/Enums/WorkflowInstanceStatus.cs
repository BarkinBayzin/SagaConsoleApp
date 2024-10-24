namespace SagaConsoleApp_v2.Entities.Enums
{
    public enum WorkflowInstanceStatus
    {

        Waiting = 0, //başlamak için bekliyor olabilir.
        Started = 10, //başlamış ya da devam ediyor olabilir.
        Rejected = 50, //iptal edilerek sonlanmış olabilir
        Ended = 100, //Olumlu sonuçlanmış olabilir
        Canceled = 200, //Dış bir etki ile iptal edilmiş olabilir.
        Error = 1000, //Hata almış ise
        NotStarted = 1001, //Listelemede kullanılır.
    }
}
