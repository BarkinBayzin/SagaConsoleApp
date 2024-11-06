using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Consumers;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Saga;
using SagaConsoleApp_v2.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Logger yapılandırması
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

try
{
    Log.Information("Uygulama başlatılıyor.");
    var configuration = builder.Configuration;
    string connectionString = configuration.GetConnectionString("DefaultConnection");
    bool useInMemoryDatabase = false; // In-Memory veritabanı kullanımı

    // Services eklenmesi
    /*
     AddScoped ve AddSingleton Arasındaki Fark:
      - Scoped: Her HTTP isteği için yeni bir örnek oluşturulur.
      - Singleton: Uygulama boyunca tek bir örnek oluşturulur ve her istek bu örneği paylaşır.
      - Transient: Her istekte yeni bir örnek oluşturulur.
     */
    //builder.Services.Configure<CrmIntegrationOptions>(configuration.GetSection(CrmIntegrationOptions.SectionName));
    builder.Services.AddScoped<IOfferService, OfferService>(); 
    builder.Services.AddScoped<WorkflowApprovedConsumer>();
    builder.Services.AddScoped<WorkflowRejectedConsumer>();
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

        cfg.AddSagaStateMachine<WorkflowSaga, WorkflowSagaState>()
         .EntityFrameworkRepository(r =>
         {
             r.ConcurrencyMode = ConcurrencyMode.Optimistic;
             r.AddDbContext<WorkflowSagaDbContext, WorkflowSagaDbContext>((provider, builder) =>
             {
                 builder.UseSqlServer(connectionString, m =>
                 {
                     m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                     m.MigrationsHistoryTable($"__{nameof(WorkflowSagaDbContext)}");
                 });
             });
         });

        cfg.AddConsumers(typeof(Program).Assembly);
        cfg.AddConsumer<StartWorkflowConsumer>(typeof(StartWorkflowConsumerDefinition));
        cfg.SetKebabCaseEndpointNameFormatter();
        #region End point conversations and definitions examples

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

    app.MapPost("/api/create-offer", async ([FromQuery] string ghTur, IBusControl busControl) =>
    {
        var correlationId = Guid.NewGuid();
        var request = new OvercapacityRequestReceived
        {
            CorrelationId = correlationId,
            GhTur = ghTur,
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

    app.MapGet("/api/get-offer", async ([FromQuery] Guid correlationId, ApplicationDbContext dbContext) =>
    {
        var sagaState = await dbContext.Set<OvercapacitySagaState>().FindAsync(correlationId);
        if (sagaState != null && sagaState.OfferId != Guid.Empty)
            return Results.Ok(new { OfferId = sagaState.OfferId });
        else
            return Results.NotFound("OfferId henüz oluşturulmadı veya CorrelationId bulunamadı.");
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

    app.MapPost("/api/start-workflow", async (Guid correlationId, Guid offerId, IPublishEndpoint publishEndpoint) =>
    {
        await publishEndpoint.Publish(new StartWorkflow
        {
            CorrelationId = correlationId,
            OfferId = offerId
        });
        return Results.Ok($"Workflow başlatıldı. CorrelationId: {correlationId}, OfferId: {offerId}");
    });

    app.MapPost("/api/approve-workflow", async (Guid correlationId, Guid offerId, IPublishEndpoint publishEndpoint) =>
    {
        await publishEndpoint.Publish(new WorkflowApproved
        {
            CorrelationId = correlationId,
            OfferId = offerId
        });
        return Results.Ok($"Workflow onaylandı. CorrelationId: {correlationId}, OfferId: {offerId}");
    });

    app.MapPost("/api/reject-workflow", async (Guid correlationId, Guid offerId, IPublishEndpoint publishEndpoint) =>
    {
        await publishEndpoint.Publish(new WorkflowRejected
        {
            CorrelationId = correlationId,
            OfferId = offerId
        });
        return Results.Ok($"Workflow reddedildi. CorrelationId: {correlationId}, OfferId: {offerId}");
    });


    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılamadı.");
}
finally
{
    Log.CloseAndFlush();
}

