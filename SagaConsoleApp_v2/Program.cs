using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Saga;
using SagaConsoleApp_v2.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logger yapılandırması
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SagaDb;Trusted_Connection=True;";
bool useInMemoryDatabase = false; // In-Memory veritabanı kullanımı

// Services eklenmesi
/*
 AddScoped ve AddSingleton Arasındaki Fark:
  - Scoped: Her HTTP isteği için yeni bir örnek oluşturulur.
  - Singleton: Uygulama boyunca tek bir örnek oluşturulur ve her istek bu örneği paylaşır.
  - Transient: Her istekte yeni bir örnek oluşturulur.
 */
builder.Services.AddScoped<OfferService>();
builder.Services.AddScoped<CrmIntegrationService>();
builder.Services.AddScoped<EmailService>();

// DbContext'leri ekleme
if (useInMemoryDatabase)
{
    builder.Services.AddDbContext<SagaStateDbContext>(options =>
        options.UseInMemoryDatabase("SagaStateDb"), ServiceLifetime.Singleton);

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("ApplicationDb"), ServiceLifetime.Singleton);
}
else
{
    builder.Services.AddDbContext<SagaStateDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// MassTransit ve Saga ayarları
builder.Services.AddMassTransit(cfg =>
{
    //cfg.AddSagaStateMachine<OvercapacitySaga, OvercapacitySagaState>()
    //    .InMemoryRepository();

    cfg.AddSagaStateMachine<OvercapacitySaga, OvercapacitySagaState>()
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
        r.AddDbContext<SagaDbContext, SagaStateDbContext>((provider, builder) =>
        {
            builder.UseSqlServer(connectionString);
        });
    });

    cfg.AddConsumers(typeof(Program).Assembly);

    #region End point conversations and definitions examples
    //cfg.SetKebabCaseEndpointNameFormatter();

    //cfg.AddConsumer<OvercapacityRequestConsumer>();
    //cfg.AddConsumer<CreateOpportunityConsumer>();
    //cfg.AddConsumer<CreateUpgradeOfferConsumer>();
    //cfg.AddConsumer<SendNotificationEmailConsumer>();
    //cfg.AddConsumer<DeleteOfferConsumer>();
    //cfg.AddConsumer<DeleteOpportunityConsumer>();

    // Endpoint conventions ile endpointleri mapleme yöntemi
    //EndpointConvention.Map<CheckOffer>(new Uri("queue:check-offer"));
    //EndpointConvention.Map<CreateUpgradeOffer>(new Uri("queue:create-upgrade-offer"));
    //EndpointConvention.Map<SendEmailNotification>(new Uri("queue:send-email-notification"));

    
    // public class CheckOfferConsumerDefinition : ConsumerDefinition<CheckOfferConsumer>
    // {
    //    public CheckOfferConsumerDefinition()
    //    {
    //        EndpointName = "check-offer";
    //    }
    //}
    #endregion

    cfg.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddHostedService<MassTransitHostedService>();

var app = builder.Build();

app.MapPost("/api/create-offer", async (IBusControl busControl) =>
{
    var correlationId = Guid.NewGuid();
    var request = new OvercapacityRequest
    {
        CorrelationId = correlationId,
        GhTur = "GHTUR--0010335-24",
        DateTriggered = DateTime.UtcNow,
        Products = new List<AutomationProduct>
        {
            new AutomationProduct
            {
                ProductId = Guid.NewGuid(),
                Name = "IaaS vRAM",
                Quantity = 32,
                Unit = new AutomationUnit { Id = Guid.NewGuid(), Name = "GB" },
                Phase = new AutomationPhase { Id = Guid.NewGuid(), Name = "Prod" }
            }
        }
    };

    await busControl.Publish(request);

    return Results.Ok(new { message = "Overcapacity request sent.", correlationId });
});

app.MapPost("/api/update-offer", async (IBusControl busControl, Guid correlationId) =>
{
    var request = new UpdateOfferRequest
    {
        CorrelationId = correlationId,
        OfferId = Guid.NewGuid(),
        UpdatedDate = DateTime.UtcNow
    };

    await busControl.Publish(request);

    return Results.Ok("Offer update request sent.");
});


app.Run();
