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
                            return;
                        }

                        var offerWorkflowHistory = await offerService.GetOrCreateOfferWorkflowHistoryAsync(offer);
                        context.Saga.OfferWorkflowHistoryId = offerWorkflowHistory.Id;

                        _logger.LogInformation("[WorkflowSaga] Workflow başlatıldı. OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Saga.OfferId, context.Saga.CorrelationId);

                        // Onay bekleme durumuna geç
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
                        offerWorkflowHistory.Approve(Guid.NewGuid(), offerWorkflowHistory.WorkflowReasons.LastOrDefault().Reason, offerWorkflowHistory.WorkflowReasons.LastOrDefault().StateType);

                        // Teklifin durumunu güncelle
                        offer.Status = Entities.Enums.WorkflowTaskStatus.Approved;

                        await offerService.UpdateOfferAsync(offer);
                        await offerService.UpdateOfferWorkflowHistoryAsync(offerWorkflowHistory);

                        // Gerekirse FinalizeWorkflow olayını yayınlayın
                        await context.Publish(new FinalizeWorkflow
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
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
                        offerWorkflowHistory.Reject(Guid.NewGuid(), offerWorkflowHistory.WorkflowReasons.LastOrDefault().Reason, offerWorkflowHistory.WorkflowReasons.LastOrDefault().StateType);

                        // Teklifin durumunu güncelle
                        offer.Status = Entities.Enums.WorkflowTaskStatus.Rejected;

                        await offerService.UpdateOfferAsync(offer);
                        await offerService.UpdateOfferWorkflowHistoryAsync(offerWorkflowHistory);

                        // Gerekirse FinalizeWorkflow olayını yayınlayın
                        await context.Publish(new FinalizeWorkflow
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId
                        });
                    })
                    .TransitionTo(Rejected)
            );

            During(Approved,
                When(FinalizeWorkflowEvent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow tamamlandı (Approved). CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        // Gerekli son işlemleri gerçekleştir
                    })
                    .TransitionTo(Completed)
            );

            During(Rejected,
                When(FinalizeWorkflowEvent)
                    .Then(context =>
                    {
                        _logger.LogInformation("[WorkflowSaga] Workflow tamamlandı (Rejected). CorrelationId: {CorrelationId}", context.Saga.CorrelationId);
                        // Gerekli son işlemleri gerçekleştir
                    })
                    .TransitionTo(Completed)
            );

            SetCompletedWhenFinalized();
        }

        public State WaitingForApproval { get; private set; }
        public State Approved { get; private set; }
        public State Rejected { get; private set; }
        public State Completed { get; private set; }

        public Event<StartWorkflow> StartWorkflowEvent { get; private set; }
        public Event<WorkflowApproved> WorkflowApprovedEvent { get; private set; }
        public Event<WorkflowRejected> WorkflowRejectedEvent { get; private set; }
        public Event<FinalizeWorkflow> FinalizeWorkflowEvent { get; private set; }
    }


}
