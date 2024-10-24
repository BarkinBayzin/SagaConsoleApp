using MassTransit;
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

        public Event<OvercapacityRequestReceived> OvercapacityRequestReceived { get; private set; }
        public Event<OvercapacityRequestAccepted> OvercapacityRequestAccepted { get; private set; }
        public Event<OvercapacityRequestRejected> OvercapacityRequestRejected { get; private set; }
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
        private readonly ILogger<OvercapacitySaga> _logger;

        public OvercapacitySaga(ILogger<OvercapacitySaga> logger)
        {
            _logger = logger;
            InstanceState(x => x.CurrentState);

            // Eventlerin CorrelationId ile eşleştirilmesi
            Event(() => OvercapacityRequestReceived, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OvercapacityRequestAccepted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OvercapacityRequestRejected, x => x.CorrelateById(context => context.Message.CorrelationId));
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

            Initially(
                When(OvercapacityRequestReceived)
                    .Then(context =>
                    {
                        context.Saga.GhTur = context.Message.GhTur;
                        _logger.LogInformation("[Saga] OvercapacityRequestReceived işlendi, GhTur: {GhTur}, CorrelationId: {CorrelationId}", context.Saga.GhTur, context.Saga.CorrelationId);
                    })
                    .PublishAsync(context => context.Init<OvercapacityRequestAccepted>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        GhTur = context.Saga.GhTur
                    }))
                    .TransitionTo(Validated)
            );

            During(Validated,
                When(OvercapacityRequestAccepted)
                    .ThenAsync(async context =>
                    {
                        // Önce mevcut CRM fırsat ID'sini kontrol et, zaten işlenmişse tekrar işleme
                        if (context.Saga.CrmOpportunityId != Guid.Empty)
                        {
                            _logger.LogWarning("Opportunity already created, skipping. CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                            return;
                        }
            
                        _logger.LogInformation("[Saga] OvercapacityRequestAcceptedEvent alındı, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        var crmService = context.GetPayload<CrmIntegrationService>();
                        var crmResult = await crmService.CreateUpgradeOpportunityAsync(context.Message.GhTur, DateTime.UtcNow);
            
                        if (crmResult.IsSuccess)
                        {
                            context.Saga.CrmOpportunityId = crmResult.Value.OpportunityId; // Idempotency için CRM fırsat ID'sini kaydediyoruz.
                            await context.Publish(new OpportunityCreated
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                CrmOpportunityId = crmResult.Value.OpportunityId
                            });
                        }
                        else
                        {
                            _logger.LogWarning("[Saga] OvercapacityRequestRejectedEvent alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", crmResult.Errors.First(),          context.Saga.CorrelationId);
                            await context.Publish(new OpportunityCreationFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                Reason = crmResult.Errors.FirstOrDefault()
                            });
                        }
                    })
                    .TransitionTo(OpportunityCreatedState)
            );


            During(OpportunityCreatedState,
                When(OpportunityCreated)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] OpportunityCreatedEvent alındı, OpportunityId: {OpportunityId}, CorrelationId: {CorrelationId}", context.Message.CrmOpportunityId, context.Saga.CorrelationId);
                        context.Saga.CrmOpportunityId = context.Message.CrmOpportunityId;

                        var offerService = context.GetPayload<IOfferService>();

                        var offerResult = await offerService.CreateUpgradeOfferAsync(Guid.NewGuid(), context.Saga.GhTur, context.Saga.CrmOpportunityId);

                        if (offerResult.IsSuccess)
                        {
                            await context.Publish(new UpgradeOfferCreated
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = offerResult.Value.Id
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

                When(OpportunityCreationFailed)
                    .Then(context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        _logger.LogError("[Saga] OpportunityCreationFailedEvent alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                    })
                    .TransitionTo(Failed)
            );

            During(OfferCreatedState,
                When(UpgradeOfferCreated)
                    .ThenAsync(async context =>
                    {
                        context.Saga.OfferId = context.Message.OfferId;
                        _logger.LogInformation("[Saga] UpgradeOfferCreatedEvent alındı, UpgradeOfferId: {UpgradeOfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Saga.CorrelationId);
                        await context.Publish(new SendNotificationEmail
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
                    })
                    .TransitionTo(EmailSentState),

                When(UpgradeOfferCreationFailed)
                    .ThenAsync(async context =>
                    {
                        _logger.LogError("[Saga] UpgradeOfferCreationFailedEvent alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                        var crmService = context.GetPayload<CrmIntegrationService>();
                        var deleteResult = await crmService.DeleteOpportunityAsync(context.Saga.CrmOpportunityId);

                        context.Saga.FailureReason = context.Message.Reason;
                        _logger.LogError("[Saga] UpgradeOfferCreationFailedEvent alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                    })
                    .TransitionTo(Failed)
            );

            During(EmailSentState,
                When(NotificationEmailSent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[Saga] NotificationEmailSentEvent alındı, Saga tamamlandı, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        context.Publish(new StartWorkflow
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
                    })
                    .TransitionTo(Completed),

                When(NotificationEmailFailed)
                    .ThenAsync(async context =>
                    {
                        context.Saga.FailureReason = context.Message.Reason;
                        _logger.LogError("[Saga] NotificationEmailFailedEvent alındı, Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);

                        await context.Publish(new DeleteOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
                    })
                    .TransitionTo(CompensationInProgress)
            );

            During(CompensationInProgress,
                When(OfferDeleted)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[Saga] OfferDeletedEvent alındı, Offer silme başarılı OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Message.OfferId, context.Saga.CorrelationId);

                        await context.Publish(new DeleteOpportunity
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OpportunityId = context.Saga.CrmOpportunityId
                        });
                    }),

                When(OfferDeletionFailed)
                    .ThenAsync(async context =>
                    {
                        _logger.LogError("[Saga] OfferDeletionFailedEvent alındı, Offer silme başarısız oldu. Reason:{Reason} UpgradeOfferId: {UpgradeOfferId}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.OfferId, context.Saga.CorrelationId);
                    }),

                When(OpportunityDeleted)
                    .Then(context =>
                    {
                        _logger.LogInformation("[Saga] OpportunityDeletedEvent alındı, Fırsat silme başarılı, CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                    })
                    .TransitionTo(Failed),

                When(OpportunityDeletionFailed)
                    .Then(context =>
                    {
                        _logger.LogError("[Saga] OpportunityDeletionFailedEvent alındı, Fırsat silme başarısız oldu. Reason: {Reason}, CorrelationId: {CorrelationId}", context.Message.Reason, context.Saga.CorrelationId);
                        context.Saga.FailureReason = "Fırsat silme başarısız oldu: " + context.Message.Reason;
                    })
                    .TransitionTo(Failed)
            );

            During(Failed,
                Ignore(NotificationEmailSent),
                Ignore(NotificationEmailFailed),
                Ignore(OvercapacityRequestAccepted),
                Ignore(OvercapacityRequestRejected),
                Ignore(OpportunityCreated),
                Ignore(OpportunityCreationFailed),
                Ignore(UpgradeOfferCreated),
                Ignore(UpgradeOfferCreationFailed)
            );

            SetCompletedWhenFinalized();
        }
    }
}
