// Consumers/CreateUpgradeOfferConsumer.cs
using MassTransit;
using Serilog;

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
            Console.WriteLine($"[Consumer] Creating Upgrade Offer for GhTur={context.Message.CheckResult.GhTur}");

            // Simulate creating upgrade offer
            await Task.Delay(500);

            var offer = new OfferRecord(
                Guid.NewGuid(),
                context.Message.CheckResult.GhTur,
                "Test Customer",
                "System",
                "FORM123",
                DateTime.Now,
                Guid.NewGuid()
            );

            // Simulate success
            await context.Publish(new UpgradeOfferCreated
            {
                CorrelationId = context.Message.CorrelationId,
                IsSuccess = true,
                Offer = offer
            });
        }
        catch (Exception ex)
        {
            // Publish a compensation request if something goes wrong
            await context.Publish(new CompensationRequest
            {
                CorrelationId = context.Message.CorrelationId,
                Reason = ex.Message
            });

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


