using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaConsoleApp_v2.Consumers;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;
using SagaConsoleApp_v2.Saga;
using SagaConsoleApp_v2.Services;
using Serilog;

namespace SagaConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Logger yapılandırması
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SagaDb;Trusted_Connection=True;";

            bool useInMemoryDatabase = false; // In-Memory veritabanı kullanımı

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<OfferService>();
                    services.AddSingleton<CrmIntegrationService>();
                    services.AddSingleton<EmailService>();

                    // DbContext'leri ekleme
                    if (useInMemoryDatabase)
                    {
                        // In-Memory Veritabanı Kullanımı
                        services.AddDbContext<SagaStateDbContext>(options =>
                            options.UseInMemoryDatabase("SagaStateDb"), ServiceLifetime.Singleton);

                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseInMemoryDatabase("ApplicationDb"), ServiceLifetime.Singleton);
                    }
                    else
                    {
                        // SQL Server Veritabanı Kullanımı
                        services.AddDbContext<SagaStateDbContext>(options =>
                            options.UseSqlServer(connectionString));

                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer(connectionString));
                    }

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddSagaStateMachine<OvercapacitySaga, OvercapacitySagaState>()
                            .InMemoryRepository();

                        cfg.AddConsumer<OvercapacityRequestConsumer>();
                        cfg.AddConsumer<CreateOpportunityConsumer>();
                        cfg.AddConsumer<CreateUpgradeOfferConsumer>();
                        cfg.AddConsumer<SendNotificationEmailConsumer>();

                        cfg.UsingInMemory((context, cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddHostedService<MassTransitHostedService>();
                })
                .Build();

            await host.StartAsync();

            var busControl = host.Services.GetRequiredService<IBusControl>();

            var correlationId = Guid.NewGuid();

            var request = new OvercapacityRequest
            {
                CorrelationId = correlationId,
                GhTur = "GHTUR--0010335-24",
                DateTriggered = DateTime.UtcNow,
                Products = new System.Collections.Generic.List<AutomationProduct>
                {
                    new AutomationProduct
                    {
                        ProductId = Guid.NewGuid(),
                        Name = "Sample Product",
                        Quantity = 1,
                        Unit = new AutomationUnit { Id = Guid.NewGuid(), Name = "Unit" },
                        Phase = new AutomationPhase { Id = Guid.NewGuid(), Name = "Phase" }
                    }
                }
            };

            await busControl.Publish(request);

            Console.WriteLine("Aşırı kapasite isteği gönderildi.");

            // Sürecin tamamlanmasını beklemek için bir süre bekleyin
            await Task.Delay(5000);

            await host.StopAsync();

            Console.ReadKey();
        }
    }
}
