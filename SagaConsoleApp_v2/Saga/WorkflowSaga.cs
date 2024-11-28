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

            Initially(
            When(StartWorkflowEvent)
                .ThenAsync(async context =>
                {
                    // Initialize saga state and send email
                    await InitializeWorkflowAsync(context);
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

                        // Submit to CRM
                        var crmService = context.GetPayload<IServiceProvider>().GetRequiredService<CrmIntegrationService>();
                        var submitResult = await crmService.SubmitOfferToCrmAsync(offer);

                        if (submitResult.IsSuccess)
                        {
                            _logger.LogInformation("[WorkflowSaga] Offer successfully submitted to CRM. OfferId: {OfferId}", context.Saga.OfferId);

                            // Publish FinalizeWorkflow event
                            await context.Publish(new FinalizeWorkflow
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                OfferId = context.Saga.OfferId
                            });

                            await context.SetCompleted();
                        }
                        else
                        {
                            _logger.LogError("[WorkflowSaga] Failed to submit offer to CRM. Reason: {Reason}", submitResult.Errors.FirstOrDefault());

                            // Publish CrmSubmitFailed event
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

                        // Publish CrmSubmitFailed event due to rejection
                        await context.Publish(new CrmSubmitFailed
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId,
                            Reason = "Workflow was rejected by the user."
                        });

                        _logger.LogInformation("[WorkflowSaga] Reddetme işlemi başarıyla tamamlandı. OfferId: {OfferId}", context.Saga.OfferId);

                        await context.SetCompleted();
                    })
                    .TransitionTo(Rejected)
            );

            SetCompletedWhenFinalized();
        }
        private async Task InitializeWorkflowAsync(BehaviorContext<WorkflowSagaState, StartWorkflow> context)
        {
            context.Saga.OfferId = context.Message.OfferId;
            _logger.LogInformation("[WorkflowSaga] Workflow started. OfferId: {OfferId}, CorrelationId: {CorrelationId}", context.Saga.OfferId, context.Saga.CorrelationId);

            var offerService = context.GetPayload<IServiceProvider>().GetRequiredService<IOfferService>();
            var offer = await offerService.GetOfferByIdAsync(context.Saga.OfferId);
            if (offer == null)
            {
                _logger.LogError("[WorkflowSaga] Offer not found. OfferId: {OfferId}", context.Saga.OfferId);
                await context.SetCompleted();
                return;
            }

            var offerWorkflowHistory = await offerService.GetOrCreateOfferWorkflowHistoryAsync(offer);
            context.Saga.OfferWorkflowHistoryId = offerWorkflowHistory.Id;

            // Send email notification to user
            var emailService = context.GetPayload<IServiceProvider>().GetRequiredService<EmailService>();
            await emailService.SendNotificationEmailAsync(offer);

            _logger.LogInformation("[WorkflowSaga] Notification email sent for OfferId: {OfferId}", context.Saga.OfferId);

            // Now waiting for user approval
        }
        public State WaitingForApproval { get; private set; }
        public State Approved { get; private set; }
        public State Rejected { get; private set; }

        public Event<StartWorkflow> StartWorkflowEvent { get; private set; }
        public Event<WorkflowApproved> WorkflowApprovedEvent { get; private set; }
        public Event<WorkflowRejected> WorkflowRejectedEvent { get; private set; }
    }

}
