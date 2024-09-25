using MassTransit;
using SagaConsoleApp.Messages;

namespace SagaConsoleApp.Saga
{
    public class OverCapacityUpgradeSaga : MassTransitStateMachine<OverCapacityUpgradeSagaState>
    {
        public State CheckingOffer { get; private set; }
        public State CreatingUpgradeOffer { get; private set; }
        public State SendingEmailNotification { get; private set; }

        public Event<OverCapacityRequestReceived> OverCapacityRequestReceivedEvent { get; private set; }
        public Event<OfferChecked> OfferCheckedEvent { get; private set; }
        public Event<UpgradeOfferCreated> UpgradeOfferCreatedEvent { get; private set; }
        public Event<EmailNotificationSent> EmailNotificationSentEvent { get; private set; }
        public Event<CompensationRequest> CompensationRequestEvent { get; set; }

        public OverCapacityUpgradeSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OverCapacityRequestReceivedEvent, x =>
                {
                    x.CorrelateById(context => context.Message.CorrelationId);
                    x.InsertOnInitial = true;
                    x.SetSagaFactory(context => new OverCapacityUpgradeSagaState
                    {
                        CorrelationId = context.Message.CorrelationId
                    });
                });
            Event(() => OfferCheckedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => UpgradeOfferCreatedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => EmailNotificationSentEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
            Event(() => CompensationRequestEvent, x => x.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(OverCapacityRequestReceivedEvent)
                    .Then(context =>
                    {
                        context.Saga.GhTur = context.Saga.GhTur;
                        context.Saga.DateTriggered = context.Message.DateTriggered;
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        Console.WriteLine($"[Saga] Received Overcapacity Request: GhTur={context.Saga.GhTur}, Date={context.Saga.DateTriggered}");
                    })
                    .TransitionTo(CheckingOffer)
                    .Publish(context => new CheckOffer(context.Saga.CorrelationId, context.Saga.GhTur, context.Saga.DateTriggered))
            );

            During(CheckingOffer,
                When(OfferCheckedEvent)
                    .IfElse(context => context.Message.IsSuccess,
                        thenBinder => thenBinder
                            .Then(context =>
                            {
                                Console.WriteLine("[Saga] Offer Checked Successfully");
                            })
                            .TransitionTo(CreatingUpgradeOffer)
                            .Publish(context => new CreateUpgradeOffer(context.Saga.CorrelationId, context.Message.Result, context.Saga.DateTriggered)),
                        elseBinder => elseBinder
                            .Then(context =>
                            {
                                context.Saga.ErrorMessage = context.Message.ErrorMessage ?? "Default Error Message";
                                Console.WriteLine($"[Saga] Offer Check Failed: {context.Saga.ErrorMessage}");
                            })
                            .Finalize()
                    )
            );

            During(CreatingUpgradeOffer,
                When(UpgradeOfferCreatedEvent)
                    .IfElse(context => context.Message.IsSuccess,
                        thenBinder => thenBinder
                            .Then(context =>
                            {
                                context.Saga.OfferId = context.Message.Offer.Id;
                                Console.WriteLine("[Saga] Upgrade Offer Created Successfully");
                            })
                            .TransitionTo(SendingEmailNotification)
                            .Publish(context => new SendEmailNotification(context.Saga.CorrelationId, context.Message.Offer)),
                        elseBinder => elseBinder
                            .Then(context =>
                            {
                                context.Saga.ErrorMessage = context.Message.ErrorMessage ?? "Default Error Message";
                                Console.WriteLine($"[Saga] Upgrade Offer Creation Failed: {context.Saga.ErrorMessage}");
                                // Eğer işlem başarısızsa, önceki işlemleri geri almak için bir olay tetiklenebilir
                                // Telafi işlemi
                                context.Publish(new CompensationRequest
                                {
                                    CorrelationId = context.Message.CorrelationId,
                                    Reason = "Email Notification Failed"
                                });
                            })
                            .Finalize()
                    ),
                When(CompensationRequestEvent)
                    .Then(context =>
                    {
                        Console.WriteLine($"[Saga] Compensation Request Received: {context.Message.Reason}");
                        Console.WriteLine("[Saga] Over Capacity Upgrage Offer State Ending..");
                    })
                    .Finalize()
            );

            During(SendingEmailNotification,
                When(EmailNotificationSentEvent)
                    .IfElse(context => context.Message.IsSuccess,
                        thenBinder => thenBinder
                            .Then(context =>
                            {
                                Console.WriteLine("[Saga] Email Notification Sent Successfully");
                                Console.WriteLine("[Saga] Over Capacity Upgrage Offer State Ending..");
                            })
                            .Finalize(),
                        elseBinder => elseBinder
                            .Then(context =>
                            {
                                context.Saga.ErrorMessage = context.Message.ErrorMessage ?? "Default Error Message";
                                Console.WriteLine($"[Saga] Email Notification Failed: {context.Saga.ErrorMessage}");
                            })
                            .Finalize()
                    )
            );

            SetCompletedWhenFinalized();
        }
    }

    //public class CapacityOverrunSaga : MassTransitStateMachine<CapacityOverrunState>
    //{
    //    public State CheckingGHTUR { get; private set; }
    //    public State CreatingOpportunity { get; private set; }
    //    public State CreatingOffer { get; private set; }
    //    public State Notifying { get; private set; }
    //    public State Completed { get; private set; }
    //    public State Failed { get; private set; }

    //    public Event<OvercapacityRequestReceived> OvercapacityRequestReceivedEvent { get; private set; }
    //    public Event<OpportunityCreated> OpportunityCreatedEvent { get; private set; }
    //    public Event<OfferCreated> OfferCreatedEvent { get; private set; }
    //    public Event<NotificationSent> NotificationSentEvent { get; private set; }
    //    public Event<CompensationTriggered> CompensationTriggeredEvent { get; private set; }

    //    public CapacityOverrunSaga()
    //    {
    //        InstanceState(x => x.CurrentState);

    //        Event(() => OvercapacityRequestReceivedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    //        Event(() => OpportunityCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    //        Event(() => OfferCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    //        Event(() => NotificationSentEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    //        Event(() => CompensationTriggeredEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

    //        Initially(
    //            When(OvercapacityRequestReceivedEvent)
    //                .Then(context =>
    //                {
    //                    // Logika ve iş akışı başlatma
    //                    Console.WriteLine("Overcapacity request received, checking GHTUR.");
    //                })
    //                .TransitionTo(CheckingGHTUR)
    //        );

    //        During(CheckingGHTUR,
    //            When(OpportunityCreatedEvent)
    //                .Then(context =>
    //                {
    //                    // Logika ve CRM ile etkileşim
    //                    Console.WriteLine("Opportunity created successfully in CRM.");
    //                })
    //                .TransitionTo(CreatingOpportunity)
    //        );

    //        During(CreatingOpportunity,
    //            When(OfferCreatedEvent)
    //                .Then(context =>
    //                {
    //                    // Teklif oluşturma
    //                    Console.WriteLine("Offer created successfully.");
    //                })
    //                .TransitionTo(CreatingOffer)
    //        );

    //        During(CreatingOffer,
    //            When(NotificationSentEvent)
    //                .Then(context =>
    //                {
    //                    // Bilgilendirme maili gönderildi
    //                    Console.WriteLine("Notification email sent.");
    //                })
    //                .TransitionTo(Notifying)
    //                .Finalize()
    //        );

    //        During(Notifying,
    //            When(CompensationTriggeredEvent)
    //                .Then(context =>
    //                {
    //                    // Hata yönetimi ve telafi mekanizması
    //                    Console.WriteLine("Error occurred, triggering compensation.");
    //                })
    //                .TransitionTo(Failed)
    //        );

    //        SetCompletedWhenFinalized();
    //    }
    //}

}


