using MassTransit;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Services;
using System.ComponentModel.DataAnnotations;

namespace SagaConsoleApp_v2.Saga
{
    public class WorkflowSagaState : SagaStateMachineInstance
    {
        [Key]
        public Guid CorrelationId { get; set; }
        public Guid OfferId { get; set; }
        public Guid OfferWorkflowHistoryId { get; set; }
        public string CurrentState { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }

    public class WorkflowSaga : MassTransitStateMachine<WorkflowSagaState>
    {
        private readonly ILogger<WorkflowSaga> _logger;

        public WorkflowSaga(ILogger<WorkflowSaga> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState);

            Event(() => StartWorkflowEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorkflowApprovedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorkflowRejectedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => FinalizeWorkflowEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => CrmSubmitFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(StartWorkflowEvent)
                    .ThenAsync(async context =>
                    {
                        context.Saga.OfferId = context.Message.OfferId;
                        var offerService = context.GetPayload<IServiceProvider>().GetRequiredService<IOfferService>();
                        var offer = await offerService.GetOfferByIdAsync(context.Saga.OfferId);
                        if (offer == null)
                        {
                            _logger.LogError("[WorkflowSaga] Teklif bulunamadı. OfferId: {OfferId}", context.Saga.OfferId);
                            await context.SetCompleted();
                            return;
                        }

                        var offerWorkflowHistory = await offerService.GetOrCreateOfferWorkflowHistoryAsync(offer);
                        context.Saga.OfferWorkflowHistoryId = offerWorkflowHistory.Id;

                        _logger.LogInformation("[WorkflowSaga] Workflow başlatıldı. OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Saga.OfferId, context.Saga.CorrelationId);

                        // Otomatik onaylama işlemi
                        await context.Publish(new WorkflowApproved
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
                    })
                    .TransitionTo(WaitingForApproval)
            );

            During(WaitingForApproval,
                When(WorkflowApprovedEvent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow onaylandı. CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        var offerService = context.GetPayload<IServiceProvider>().GetRequiredService<IOfferService>();
                        var offer = await offerService.GetOfferByIdAsync(context.Saga.OfferId);
                        var offerWorkflowHistory = await offerService.GetOfferWorkflowHistoryByIdAsync(context.Saga.OfferWorkflowHistoryId);

                        // İş akışını güncelle
                        offerWorkflowHistory.Approve(Guid.NewGuid(), null, Entities.Enums.StateType.Technical);

                        // Teklifin durumunu güncelle
                        offer.Status = Entities.Enums.WorkflowTaskStatus.Approved;

                        await offerService.UpdateOfferAsync(offer);
                        await offerService.UpdateOfferWorkflowHistoryAsync(offerWorkflowHistory);

                        // CRM'e submit işlemi
                        var crmService = context.GetPayload<IServiceProvider>().GetRequiredService<CrmIntegrationService>();
                        var submitResult = await crmService.SubmitOfferToCrmAsync(offer);

                        if (submitResult.IsSuccess)
                        {
                            _logger.LogInformation("[WorkflowSaga] Teklif CRM'e başarıyla gönderildi. OfferId: {OfferId}", context.Saga.OfferId);

                            // Workflow tamamlandı
                            await context.Publish(new FinalizeWorkflow
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = context.Saga.OfferId
                            });

                            await context.SetCompleted();
                        }
                        else
                        {
                            _logger.LogError("[WorkflowSaga] Teklif CRM'e gönderilemedi. Reason: {Reason}", submitResult.Errors.FirstOrDefault());

                            // CrmSubmitFailedEvent yayınlayın
                            await context.Publish(new CrmSubmitFailed
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = context.Saga.OfferId,
                                Reason = submitResult.Errors.FirstOrDefault()
                            });

                            await context.SetCompleted();
                        }
                    })
                    .TransitionTo(Approved),

                When(WorkflowRejectedEvent)
                    .ThenAsync(async context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow reddedildi. CorrelationId: {CorrelationId}", context.Saga.CorrelationId);

                        var offerService = context.GetPayload<IServiceProvider>().GetRequiredService<IOfferService>();
                        var offer = await offerService.GetOfferByIdAsync(context.Saga.OfferId);
                        var offerWorkflowHistory = await offerService.GetOfferWorkflowHistoryByIdAsync(context.Saga.OfferWorkflowHistoryId);

                        // İş akışını güncelle
                        offerWorkflowHistory.Reject(Guid.NewGuid(), null, Entities.Enums.StateType.Technical);

                        // Teklifin durumunu güncelle
                        offer.Status = Entities.Enums.WorkflowTaskStatus.Rejected;

                        await offerService.UpdateOfferAsync(offer);
                        await offerService.UpdateOfferWorkflowHistoryAsync(offerWorkflowHistory);

                        // Gerekirse telafi işlemleri
                        await context.Publish(new DeleteOffer
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });

                        await context.SetCompleted();
                    })
                    .TransitionTo(Rejected)
            );

            During(Approved,
                When(FinalizeWorkflowEvent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow tamamlandı (Approved). CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        context.SetCompleted();
                    })
            );

            During(Rejected,
                When(FinalizeWorkflowEvent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow tamamlandı (Rejected). CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        context.SetCompleted();
                    })
            );

            SetCompletedWhenFinalized();
        }

        public State WaitingForApproval { get; private set; }
        public State Approved { get; private set; }
        public State Rejected { get; private set; }

        public Event<StartWorkflow> StartWorkflowEvent { get; private set; }
        public Event<WorkflowApproved> WorkflowApprovedEvent { get; private set; }
        public Event<WorkflowRejected> WorkflowRejectedEvent { get; private set; }
        public Event<FinalizeWorkflow> FinalizeWorkflowEvent { get; private set; }
        public Event<CrmSubmitFailed> CrmSubmitFailedEvent { get; private set; }
    }
}
