using MassTransit;
using SagaConsoleApp_v2.Messages;

namespace SagaConsoleApp_v2.Saga
{
    public class WorkflowSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
        public string CurrentState { get; set; }
    }

    public class WorkflowSaga : MassTransitStateMachine<WorkflowSagaState>
    {
        public State WaitingForApproval { get; private set; }
        public State Approved { get; private set; }
        public State Rejected { get; private set; }
        public State Completed { get; private set; }

        public Event<StartWorkflow> StartWorkflowEvent { get; private set; }
        public Event<WorkflowApproved> WorkflowApprovedEvent { get; private set; }
        public Event<WorkflowRejected> WorkflowRejectedEvent { get; private set; }

        public WorkflowSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => StartWorkflowEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorkflowApprovedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorkflowRejectedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(StartWorkflowEvent)
                    .Then(context =>
                    {
                        context.Saga.OfferId = context.Message.OfferId;
                        Console.WriteLine($"Workflow başlatıldı. OfferId: {context.Saga.OfferId}");
                        // Onay bekleme durumuna geç  
                    })
                    .TransitionTo(WaitingForApproval)
            );
            ///Offer wf oluşturulucak, instance atanacak ama ended yapılamyak çünkü crme submit yapıyor olmam lazım
            ///crm sonrasında wf ended yapılır
            //bir context ile saga tetiklerim
            ///birinci adımda tabloya kayıt atılır
            ///ikinci adımda kayıt atılan flagin değişmesini bekleri, true olduğunda db de manuel update yapılacak
            ///minimal api ile değişen bu flag yakalanır ve bus servis ile sagaya haber gönderilir.
            ///sonrasında 2 işlemim var tabloda bir craete etmek ve submit işlemi
            ///bu noktada bir throw atılıp compansation testleri yapılıacak
            /// iki tane minimal api olacak biri create biri update postman ile tetiklenecek bu sayede 2. saganın tutarlı devap ettiği kontrol edilecek create ise requestin bana geldiğini temsil edecek

            During(WaitingForApproval,
                When(WorkflowApprovedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("Workflow onaylandı.");
                        // Onay işlemleri gerçekleştir  
                    })
                    .TransitionTo(Approved),

                When(WorkflowRejectedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("Workflow reddedildi.");
                        // Reddetme işlemleri gerçekleştir  
                    })
                    .TransitionTo(Rejected)
            );

            During(WaitingForApproval,
                When(WorkflowApprovedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("Workflow onaylandı.");
                        Console.WriteLine("Workflow tamamlandı (Approved).");
                    }).TransitionTo(Completed),


                When(WorkflowRejectedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("Workflow reddedildi.");
                        Console.WriteLine("Workflow tamamlandı (Rejected).");
                    }).TransitionTo(Completed)
            );
        }
    }
}
