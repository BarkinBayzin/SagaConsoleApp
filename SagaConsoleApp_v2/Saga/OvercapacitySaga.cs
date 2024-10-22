using MassTransit;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Saga
{
    public class OvercapacitySaga : MassTransitStateMachine<OvercapacitySagaState>
    {
        public State Validated { get; private set; }
        public State OpportunityCreatedState { get; private set; }
        public State OfferCreatedState { get; private set; }
        public State EmailSentState { get; private set; }
        public State CompensationInProgress { get; private set; }
        public State Completed { get; private set; }
        public State Failed { get; private set; }

        public Event<OvercapacityRequest> OvercapacityRequestReceived { get; private set; }
        public Event<OvercapacityRequestAccepted> OvercapacityRequestAcceptedEvent { get; private set; }
        public Event<OvercapacityRequestRejected> OvercapacityRequestRejectedEvent { get; private set; }
        public Event<OpportunityCreated> OpportunityCreatedEvent { get; private set; }
        public Event<OpportunityCreationFailed> OpportunityCreationFailedEvent { get; private set; }
        public Event<UpgradeOfferCreated> UpgradeOfferCreatedEvent { get; private set; }
        public Event<UpgradeOfferCreationFailed> UpgradeOfferCreationFailedEvent { get; private set; }
        public Event<NotificationEmailSent> NotificationEmailSentEvent { get; private set; }
        public Event<NotificationEmailFailed> NotificationEmailFailedEvent { get; private set; }
        public Event<OfferDeleted> OfferDeletedEvent { get; private set; }
        public Event<OfferDeletionFailed> OfferDeletionFailedEvent { get; private set; }
        public Event<OpportunityDeleted> OpportunityDeletedEvent { get; private set; }
        public Event<OpportunityDeletionFailed> OpportunityDeletionFailedEvent { get; private set; }

        public OvercapacitySaga()
        {
            InstanceState(x => x.CurrentState);

            // Eventlerin CorrelationId ile eşleştirilmesi
            Event(() => OvercapacityRequestReceived, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OvercapacityRequestAcceptedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OvercapacityRequestRejectedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityCreationFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => UpgradeOfferCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => UpgradeOfferCreationFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationEmailSentEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationEmailFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OfferDeletedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OfferDeletionFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityDeletedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityDeletionFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(OvercapacityRequestReceived)
                    .Then(context =>
                    {
                        context.Saga.GhTur = context.Message.GhTur;
                    })
                    .PublishAsync(context => context.Init<OvercapacityRequestAccepted>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        GhTur = context.Saga.GhTur
                    }))
                    .TransitionTo(Validated)
            );

            During(Validated,
                When(OvercapacityRequestAcceptedEvent)
                    .ThenAsync(async context =>
                    {
                        var crmService = context.GetPayload<CrmIntegrationService>();
                        var crmResult = await crmService.CreateUpgradeOpportunityAsync(context.Message.GhTur, DateTime.UtcNow);

                        if (crmResult.IsSuccess)
                        {
                            await context.Publish(new OpportunityCreated
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                Opportunity = crmResult.Value
                            });
                        }
                        else
                        {
                            await context.Publish(new OpportunityCreationFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                Reason = crmResult.Errors.FirstOrDefault()
                            });
                        }
                    })
                    .TransitionTo(OpportunityCreatedState),

                When(OvercapacityRequestRejectedEvent)
                    .Then(context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        Console.WriteLine($"GhTur doğrulama başarısız: {context.Message.Reason}");
                    })
                    .TransitionTo(Failed)
            );

            During(OpportunityCreatedState,
                When(OpportunityCreatedEvent)
                    .ThenAsync(async context =>
                    {
                        context.Saga.CrmOpportunity = context.Message.Opportunity;

                        var offerService = context.GetPayload<OfferService>();
                        var request = new OvercapacityRequest
                        {
                            GhTur = context.Saga.GhTur,
                            DateTriggered = DateTime.UtcNow,
                            Products = new System.Collections.Generic.List<AutomationProduct>() // Ürün listesi
                        };

                        var offerResult = await offerService.CreateUpgradeOfferAsync(Guid.NewGuid(), request, context.Saga.CrmOpportunity);

                        if (offerResult.IsSuccess)
                        {
                            await context.Publish(new UpgradeOfferCreated
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                UpgradeOffer = offerResult.Value
                            });
                        }
                        else
                        {
                            await context.Publish(new UpgradeOfferCreationFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                Reason = offerResult.Errors.FirstOrDefault()
                            });
                        }
                    })
                    .TransitionTo(OfferCreatedState),

                When(OpportunityCreationFailedEvent)
                    .Then(context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        Console.WriteLine($"CRM fırsatı oluşturulamadı: {context.Message.Reason}");
                    })
                    .TransitionTo(Failed)
            );

            During(OfferCreatedState,
                When(UpgradeOfferCreatedEvent)
                    .ThenAsync(async context =>
                    {
                        context.Saga.UpgradeOffer = context.Message.UpgradeOffer;

                        var emailService = context.GetPayload<EmailService>();
                        var emailResult = await emailService.SendOvercapacityNotificationAsync(context.Saga.UpgradeOffer, null);

                        if (emailResult.IsSuccess)
                        {
                            await context.Publish(new NotificationEmailSent
                            {
                                CorrelationId = context.Saga.CorrelationId
                            });
                        }
                        else
                        {
                            await context.Publish(new NotificationEmailFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                Reason = emailResult.Errors.FirstOrDefault()
                            });
                        }
                    })
                    .TransitionTo(EmailSentState),

                When(UpgradeOfferCreationFailedEvent)
                    .ThenAsync(async context =>
                    {
                        var crmService = context.GetPayload<CrmIntegrationService>();
                        var deleteResult = await crmService.DeleteOpportunityAsync(context.Saga.CrmOpportunity.OpportunityId);

                        if (deleteResult.IsSuccess)
                        {
                            Console.WriteLine("CRM fırsatı silindi.");
                        }
                        else
                        {
                            Console.WriteLine($"CRM fırsatı silinirken hata oluştu: {deleteResult.Errors.FirstOrDefault()}");
                            // Hata loglama veya bildirim yapılabilir
                        }

                        context.Saga.FailureReason = context.Message.Reason;
                        Console.WriteLine($"Upgrade teklifi oluşturulamadı: {context.Message.Reason}");
                    })
                    .TransitionTo(Failed)
            );

            During(EmailSentState,
                When(NotificationEmailSentEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("Bildirim e-postası başarıyla gönderildi. Saga tamamlandı.");

                        // İkinci saga (WorkflowSaga) başlatılabilir
                        // Bunun için bir event yayınlayabiliriz
                        context.Publish(new StartWorkflow
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.UpgradeOffer.Id
                        });
                    })
                    .TransitionTo(Completed),

                When(NotificationEmailFailedEvent)
                    .ThenAsync(async context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        Console.WriteLine($"Bildirim e-postası gönderilemedi: {context.Message.Reason}");

                        // Compensation işlemlerini başlat
                        await context.Publish(new DeleteOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.UpgradeOffer.Id
                        });
                    })
                    .TransitionTo(CompensationInProgress)
            );

            During(CompensationInProgress,
                When(OfferDeletedEvent)
                    .ThenAsync(async context =>
                    {
                        Console.WriteLine("Offer başarıyla silindi.");

                        // Offer silme başarılı, şimdi CRM fırsatını sil
                        await context.Publish(new DeleteOpportunity
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OpportunityId = context.Saga.CrmOpportunity.OpportunityId
                        });
                    }),

                When(OfferDeletionFailedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine($"Offer silinirken hata oluştu: {context.Message.Reason}");
                        // Hata loglama veya bildirim yapılabilir
                    }),

                When(OpportunityDeletedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine("CRM fırsatı başarıyla silindi.");
                        // Compensation işlemi tamamlandı
                    })
                    .TransitionTo(Failed),

                When(OpportunityDeletionFailedEvent)
                    .Then(context =>
                    {
                        Console.WriteLine($"CRM fırsatı silinirken hata oluştu: {context.Message.Reason}");
                        // Hata loglama veya bildirim yapılabilir
                    })
                    .TransitionTo(Failed)
            );

            During(Failed,
                Ignore(NotificationEmailSentEvent),
                Ignore(NotificationEmailFailedEvent),
                Ignore(OvercapacityRequestAcceptedEvent),
                Ignore(OvercapacityRequestRejectedEvent),
                Ignore(OpportunityCreatedEvent),
                Ignore(OpportunityCreationFailedEvent),
                Ignore(UpgradeOfferCreatedEvent),
                Ignore(UpgradeOfferCreationFailedEvent)
            );

            SetCompletedWhenFinalized();
        }
    }
}
