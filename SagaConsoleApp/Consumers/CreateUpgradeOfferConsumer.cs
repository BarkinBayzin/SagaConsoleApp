using MassTransit;
using SagaConsoleApp.Data;
using SagaConsoleApp.Messages;
using SagaConsoleApp.Models;
using System.Transactions;

namespace SagaConsoleApp.Consumers
{
    public class CreateUpgradeOfferConsumer : IConsumer<CreateUpgradeOffer>
    {
        private readonly ApplicationDbContext _dbContext;

        public CreateUpgradeOfferConsumer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<CreateUpgradeOffer> context)
        {
            try
            {
                Console.WriteLine($"[Consumer] Upgrade Teklifi Oluşturuluyor: GhTur={context.Message.CheckResult.GhTur}");

                if (context.Message.CheckResult.GhTur == "FAIL_CREATE_OFFER")
                {
                    await context.Publish(new UpgradeOfferCreated(context.Message.CorrelationId, false, "Upgrade teklifi oluşturma başarısız oldu", null));
                    return;
                }

                // Başarılı sonuç
                var offer = new Offer
                {
                    Id = Guid.NewGuid(),
                    GhTur = context.Message.CheckResult.GhTur,
                    CustomerName = "Test Müşteri",
                    Creator = "Sistem",
                    FormNumber = "FORM123",
                    CreateDate = DateTime.Now,
                    CreatedById = Guid.NewGuid()
                };
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        _dbContext.Offers.Add(offer);

                        await _dbContext.SaveChangesAsync();

                        var offerDto = new OfferDto(offer.Id, offer.GhTur, offer.CustomerName, offer.Creator, offer.FormNumber, offer.CreateDate, offer.CreatedById);

                        await context.Publish(new UpgradeOfferCreated(context.Message.CorrelationId, true, null, offerDto));

                        // İşlemler başarılıysa transaction'u tamamla
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        // Hata durumunda transaction geri alınır, işlemler rollback edilir
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateUpgradeOfferConsumer] Hata oluştu: {ex.Message}");
                Console.WriteLine($"[CreateUpgradeOfferConsumer] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CreateUpgradeOfferConsumer] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }

}
