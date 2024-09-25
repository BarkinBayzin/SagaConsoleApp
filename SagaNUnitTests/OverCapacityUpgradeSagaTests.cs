using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SagaConsoleApp.Consumers;
using SagaConsoleApp.Data;
using SagaConsoleApp.Messages;
using SagaConsoleApp.Models;
using SagaConsoleApp.Saga;
using Xunit;

namespace SagaNUnitTests
{
    public class OverCapacityUpgradeSagaTests : IAsyncLifetime
    {
        private ServiceProvider _provider;
        private ITestHarness _harness;
        private ISagaStateMachineTestHarness<OverCapacityUpgradeSaga, OverCapacityUpgradeSagaState> _sagaHarness;

        public async Task InitializeAsync()
        {
            _provider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("test_db");
                })
                .AddMassTransitTestHarness(cfg =>
                {
                    cfg.AddConsumer<CheckOfferConsumer>();
                    cfg.AddConsumer<CreateUpgradeOfferConsumer>();
                    cfg.AddConsumer<SendEmailNotificationConsumer>();

                    cfg.AddSagaStateMachine<OverCapacityUpgradeSaga, OverCapacityUpgradeSagaState>()
                        .InMemoryRepository();
                })
                .BuildServiceProvider(true);

            _harness = _provider.GetRequiredService<ITestHarness>();

            // Set activity timeout to 5 seconds
            _harness.TestTimeout = TimeSpan.FromSeconds(10);
            _harness.TestInactivityTimeout = TimeSpan.FromSeconds(5);

            _sagaHarness = _harness.GetSagaStateMachineHarness<OverCapacityUpgradeSaga, OverCapacityUpgradeSagaState>();

            await _harness.Start();
        }

        public async Task DisposeAsync()
        {
            await _harness.Stop();
            await _provider.DisposeAsync();
        }

        /// <summary>
        /// Başarılı İş Akışı Testi
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Complete_Successfully()
        {
            var correlationId = Guid.NewGuid();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "GHTUR-0010340-24",
                DateTriggered = DateTime.Now
            });

            // Wait for messages to be consumed
            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            // Check if the saga instance was created
            var instanceId = await _sagaHarness.Exists(correlationId, x => x.CheckingOffer);
            Xunit.Assert.NotNull(instanceId);

            // Wait for the saga to reach the final state
            var instance = await _sagaHarness.Exists(correlationId, x => x.Final);
            //Xunit.Assert.NotNull(instance);

            // Retrieve the saga instance
            var sagaInstance = _sagaHarness.Sagas.Contains(correlationId);
            Xunit.Assert.NotNull(sagaInstance);

            //Xunit.Assert.Null(sagaInstance.CurrentState);
        }


        /// <summary>
        /// Teklif Kontrol Adımında Başarısızlık Testi
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Handle_CheckOffer_Failure()
        {
            var correlationId = Guid.NewGuid();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "FAIL_CHECK_OFFER",
                DateTriggered = DateTime.Now
            });

            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            var instance = _sagaHarness.Created.Contains(correlationId);

            Xunit.Assert.NotNull(instance);

            // Saga'nın finalize edilmesini bekleyin
            instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final);

            Xunit.Assert.NotNull(instance);

            Xunit.Assert.Equal("Teklif kontrolü başarısız oldu", instance.ErrorMessage);
        }

        /// <summary>
        /// Upgrade Teklifi Oluşturma Adımında Başarısızlık Testi
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Handle_CreateUpgradeOffer_Failure()
        {
            var correlationId = Guid.NewGuid();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "FAIL_CREATE_OFFER",
                DateTriggered = DateTime.Now
            });

            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            var instance = _sagaHarness.Created.Contains(correlationId);

            Xunit.Assert.NotNull(instance);

            // Saga'nın finalize edilmesini bekleyin
            instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final);

            Xunit.Assert.NotNull(instance);

            Xunit.Assert.Equal("Upgrade teklifi oluşturma başarısız oldu", instance.ErrorMessage);
        }

        /// <summary>
        /// E-posta Bildirimi Gönderme Adımında Başarısızlık Testi
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Handle_SendEmailNotification_Failure()
        {
            var correlationId = Guid.NewGuid();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "FAIL_SEND_EMAIL",
                DateTriggered = DateTime.Now
            });

            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            var instance = _sagaHarness.Created.Contains(correlationId);

            Xunit.Assert.NotNull(instance);

            // Saga'nın finalize edilmesini bekleyin
            instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final);

            Xunit.Assert.NotNull(instance);

            Xunit.Assert.Equal("E-posta gönderimi başarısız oldu", instance.ErrorMessage);
        }

        /// <summary>
        ///  Olumsuz Cevap Alındığında Transaction'ın Başa Dönmesi Testi
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Rollback_When_CreateUpgradeOffer_Fails()
        {
            var correlationId = Guid.NewGuid();

            var dbContext = _provider.GetRequiredService<ApplicationDbContext>();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "FAIL_CREATE_OFFER",
                DateTriggered = DateTime.Now
            });

            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            var instance = _sagaHarness.Created.Contains(correlationId);

            Xunit.Assert.NotNull(instance);

            // Saga'nın finalize edilmesini bekleyin
            instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final);

            Xunit.Assert.NotNull(instance);

            // Veritabanında teklif kaydının olmadığını doğrula
            var offer = await dbContext.Offers.FirstOrDefaultAsync(o => o.GhTur == "FAIL_CREATE_OFFER");

            Xunit.Assert.Null(offer);
        }

        /// <summary>
        /// Teklif Zaten Var Durumu
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Handle_Existing_Offer()
        {
            var correlationId = Guid.NewGuid();

            // Mevcut bir teklif oluşturun
            var dbContext = _provider.GetRequiredService<ApplicationDbContext>();
            var existingOffer = new Offer
            {
                Id = Guid.NewGuid(),
                GhTur = "EXISTING_OFFER",
                CustomerName = "Mevcut Müşteri",
                Creator = "Sistem",
                FormNumber = "FORM456",
                CreateDate = DateTime.Now,
                CreatedById = Guid.NewGuid()
            };
            dbContext.Offers.Add(existingOffer);
            await dbContext.SaveChangesAsync();

            await _harness.Bus.Publish<OverCapacityRequestReceived>(new
            {
                CorrelationId = correlationId,
                GhTur = "EXISTING_OFFER",
                DateTriggered = DateTime.Now
            });

            Xunit.Assert.True(await _harness.Consumed.Any<OverCapacityRequestReceived>(x => x.Context.Message.CorrelationId == correlationId));

            var instance = _sagaHarness.Created.Contains(correlationId);

            Xunit.Assert.NotNull(instance);

            instance = _sagaHarness.Sagas.ContainsInState(correlationId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final);

            Xunit.Assert.NotNull(instance);
        }

        /// <summary>
        /// Sadece CheckOfferConsumer'ın mesaj alıp almadığını test eder.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckOfferConsumer_Should_Consume_CheckOffer_Message()
        {
            var correlationId = Guid.NewGuid();

            await _harness.Bus.Publish<CheckOffer>(new
            {
                CorrelationId = correlationId,
                GhTur = "TEST_GH_TUR"
            });

            Xunit.Assert.True(await _harness.Consumed.Any<CheckOffer>(x => x.Context.Message.CorrelationId == correlationId));
            //Xunit.Assert.True(await _harness.Consumer<CheckOfferConsumer>().Consumed.Any<CheckOffer>(x => x.Context.Message.CorrelationId == correlationId));
        }

    }
}