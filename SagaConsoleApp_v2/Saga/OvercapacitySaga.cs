using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;

namespace SagaConsoleApp_v2.Saga
{
    public class OvercapacitySagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string GhTur { get; set; }
        public Guid CrmOpportunityId { get; set; }
        public Guid InitialOfferId { get; set; }
        public string CurrentState { get; set; }
        public string? FailureReason { get; set; }
        public bool IsCompleted { get; set; }
        public Guid OfferId { get; set; }
    }

    public class OvercapacitySaga : MassTransitStateMachine<OvercapacitySagaState>
    {
        // Durumlar
        public State Validated { get; private set; }
        public State OpportunityCreatedState { get; private set; }
        public State OfferCreatedState { get; private set; }
        public State EmailSentState { get; private set; }
        public State CompensationInProgress { get; private set; }
        public State Completed { get; private set; }
        public State Failed { get; private set; }
        public State WaitingForWorkflowCompletion { get; private set; }
        // Eventler
        public Event<OvercapacityRequestReceived> OvercapacityRequestReceived { get; private set; }
        public Event<OpportunityCreated> OpportunityCreated { get; private set; }
        public Event<OpportunityCreationFailed> OpportunityCreationFailed { get; private set; }
        public Event<UpgradeOfferCreated> UpgradeOfferCreated { get; private set; }
        public Event<UpgradeOfferCreationFailed> UpgradeOfferCreationFailed { get; private set; }
        public Event<NotificationEmailSent> NotificationEmailSent { get; private set; }
        public Event<NotificationEmailFailed> NotificationEmailFailed { get; private set; }
        public Event<OfferDeleted> OfferDeleted { get; private set; }
        public Event<OfferDeletionFailed> OfferDeletionFailed { get; private set; }
        public Event<OpportunityDeleted> OpportunityDeleted { get; private set; }
        public Event<OpportunityDeletionFailed> OpportunityDeletionFailed { get; private set; }
        public Event<CrmSubmitFailed> CrmSubmitFailedEvent { get; private set; }
        public Event<DeleteOffer> DeleteOfferEvent { get; private set; }
        public Event<FinalizeWorkflow> FinalizeWorkflowEvent { get; private set; }

        private readonly ILogger<OvercapacitySaga> _logger;

        public OvercapacitySaga(ILogger<OvercapacitySaga> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState);

            // Eventlerin CorrelationId ile eşleştirilmesi
            Event(() => OvercapacityRequestReceived, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityCreationFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => UpgradeOfferCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => UpgradeOfferCreationFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationEmailSent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationEmailFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OfferDeleted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OfferDeletionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityDeleted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OpportunityDeletionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => CrmSubmitFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => DeleteOfferEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => FinalizeWorkflowEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

            // Başlangıç durumu
            Initially(
                When(OvercapacityRequestReceived)
                    .Then(context =>
                    {
                        context.Saga.GhTur = context.Message.GhTur;
                        _logger.LogInformation("[Saga] OvercapacityRequestReceived işlendi, GhTur: {GhTur}, CorrelationId: {CorrelationId}", context.Saga.GhTur, context.Saga.CorrelationId);
                    })
                    .SendAsync(new Uri("queue:create-opportunity"), context => context.Init<CreateOpportunity>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        GhTur = context.Saga.GhTur
                    }))
                    .TransitionTo(Validated)
            );

            // Validated durumu
            During(Validated,
                When(OpportunityCreated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] OpportunityCreated alındı, OpportunityId: {OpportunityId}, CorrelationId: {CorrelationId}", context.Message.CrmOpportunityId, context.Saga.CorrelationId);
                        context.Saga.CrmOpportunityId = context.Message.CrmOpportunityId;

                        // CreateUpgradeOffer komutunu gönder
                        await context.Send(new Uri("queue:create-upgrade-offer"), new CreateUpgradeOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            GhTur = context.Saga.GhTur,
                            CrmOpportunityId = context.Saga.CrmOpportunityId
                        });

                        await context.TransitionToState(OpportunityCreatedState);
                    }),

                When(OpportunityCreationFailed)
                    .Then(context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        _logger.LogError("[Saga] OpportunityCreationFailed alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                        context.TransitionToState(Failed);
                    })
            );

            // OpportunityCreatedState durumu
            During(OpportunityCreatedState,
                When(UpgradeOfferCreated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] UpgradeOfferCreated alındı, OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Saga.CorrelationId);
                        context.Saga.OfferId = context.Message.OfferId;

                        // SendNotificationEmail komutunu gönder
                        await context.Send(new Uri("queue:send-notification-email"), new SendNotificationEmail
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });

                        await context.TransitionToState(OfferCreatedState);
                    }),

                When(UpgradeOfferCreationFailed)
                    .ThenAsync(async context =>
                    {
                        _logger.LogError("[Saga] UpgradeOfferCreationFailed alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);

                        // DeleteOpportunity komutunu gönder
                        await context.Send(new Uri("queue:delete-opportunity"), new DeleteOpportunity
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OpportunityId = context.Saga.CrmOpportunityId
                        });

                        await context.TransitionToState(CompensationInProgress);
                    })
            );

            // OfferCreatedState durumu
            During(OfferCreatedState,
                When(UpgradeOfferCreated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] UpgradeOfferCreated alındı, OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Saga.CorrelationId);
                        context.Saga.OfferId = context.Message.OfferId;

                        // SendNotificationEmail komutunu gönder
                        await context.Send(new Uri("queue:send-notification-email"), new SendNotificationEmail
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });

                        await context.TransitionToState(EmailSentState);
                    })
            );

            During(EmailSentState,
                When(NotificationEmailSent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] NotificationEmailSent alındı, Workflow başlatılıyor, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);

                        await context.Publish(new StartWorkflow
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });

                        await context.TransitionToState(WaitingForWorkflowCompletion);
                    })
            );

            During(WaitingForWorkflowCompletion,
                When(FinalizeWorkflowEvent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[Saga] FinalizeWorkflow received, completing saga, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        context.SetCompleted();
                    }),
                When(CrmSubmitFailedEvent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogError("[Saga] CrmSubmitFailed received, starting compensation actions,       CorrelationId:    {CorrelationId}", context.Saga.CorrelationId);
            
                        await context.Publish(new DeleteOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
            
                        await context.TransitionToState(CompensationInProgress);
                    })
            );


            // CompensationInProgress durumu
            During(CompensationInProgress,
                When(OfferDeleted)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] OfferDeleted alındı, OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Saga.CorrelationId);

                        // DeleteOpportunity komutunu gönder
                        await context.Send(new Uri("queue:delete-opportunity"), new DeleteOpportunity
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OpportunityId = context.Saga.CrmOpportunityId
                        });
                    }),

                When(OpportunityDeleted)
                    .Then(context =>
                    {
                        _logger.LogInformation("[Saga] OpportunityDeleted alındı, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        context.SetCompleted();
                    }),

                When(OfferDeletionFailed)
                    .Then(context =>
                    {
                        _logger.LogError("[Saga] OfferDeletionFailed alındı, Reason: {Reason}, OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Message.OfferId, context.Saga.CorrelationId);
                    }),

                When(OpportunityDeletionFailed)
                    .Then(context =>
                    {
                        _logger.LogError("[Saga] OpportunityDeletionFailed alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                        context.Saga.FailureReason = "Fırsat silme başarısız oldu: " + context.Message.Reason;
                    })
            );

            // Her durumda yakalanacak eventler
            DuringAny(
                When(CrmSubmitFailedEvent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogError("[Saga] CrmSubmitFailedEvent alındı, telafi işlemleri başlatılıyor, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);

                        // DeleteOffer komutunu gönder
                        await context.Send(new Uri("queue:delete-offer"), new DeleteOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });

                        await context.TransitionToState(CompensationInProgress);
                    }),

                When(DeleteOfferEvent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] DeleteOfferEvent alındı, telafi işlemleri başlatılıyor, CorrelationId: {CorrelationId}", context.Message.CorrelationId);

                        // Teklifi sil
                        var offerService = context.GetPayload<IOfferService>();
                        var result = await offerService.DeleteOfferAsync(context.Message.OfferId);

                        if (result.IsSuccess)
                        {
                            _logger.LogInformation("[Saga] Teklif silindi, OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Message.CorrelationId);

                            // OfferDeleted eventini yayınla
                            await context.Publish(new OfferDeleted
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = context.Message.OfferId
                            });

                            await context.TransitionToState(CompensationInProgress);
                        }
                        else
                        {
                            _logger.LogError("[Saga] Teklif silme başarısız oldu, Reason: {Reason}, CorrelationId: {CorrelationId}", result.Errors.FirstOrDefault(), context.Message.CorrelationId);

                            // OfferDeletionFailed eventini yayınla
                            await context.Publish(new OfferDeletionFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = context.Message.OfferId,
                                Reason = result.Errors.FirstOrDefault()
                            });
                        }
                    })
            );

            SetCompletedWhenFinalized();
        }
    }
}