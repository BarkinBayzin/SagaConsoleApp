using MassTransit;

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
}


