using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaConsoleApp;
using SagaConsoleApp.Saga;
using Serilog;

//Logger yapılandırması(isteğe bağlı)
Log.Logger = new LoggerConfiguration()
     .WriteTo.Console()
     .CreateLogger();

string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SagaDb;Trusted_Connection=True;";

// Kullanmak istediğiniz veritabanı türünü belirleyin
bool useInMemoryDatabase = true; // true ise in-memory veritabanı kullanılır

// Host oluşturma
var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((hostContext, services) =>
    {
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
           
            // Saga State Machine ekleme
            if (useInMemoryDatabase)
            {
                cfg.AddSagaStateMachine<OverCapacityUpgradeSaga, OverCapacityUpgradeSagaState>()
                    .InMemoryRepository();
            }
            else
            {
                cfg.AddSagaStateMachine<OverCapacityUpgradeSaga, OverCapacityUpgradeSagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ConcurrencyMode = ConcurrencyMode.Optimistic;

                        r.AddDbContext<DbContext, SagaStateDbContext>((provider, builder) =>
                        {
                            builder.UseSqlServer(connectionString, m =>
                            {
                                m.MigrationsAssembly(typeof(SagaStateDbContext).Assembly.FullName);
                            });
                        });
                    });
            }

            // Consumers ekleme
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(typeof(Program).Assembly);

            //cfg.AddConsumer<CheckOfferConsumer>(typeof(CheckOfferConsumerDefinition));
            //cfg.AddConsumer<CreateUpgradeOfferConsumer>(typeof(CreateUpgradeOfferConsumerDefinition));
            //cfg.AddConsumer<SendEmailNotificationConsumer>(typeof(SendEmailNotificationConsumerDefinition));

            // Endpoint conventions ile endpointleri mapleme yöntemi
            //EndpointConvention.Map<CheckOffer>(new Uri("queue:check-offer"));
            //EndpointConvention.Map<CreateUpgradeOffer>(new Uri("queue:create-upgrade-offer"));
            //EndpointConvention.Map<SendEmailNotification>(new Uri("queue:send-email-notification"));

            // MassTransit'i In-Memory Transport ile yapılandırma
            cfg.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        // BusHostedService'i ekleme
        services.AddHostedService<BusHostedService>();
    })
    .Build();

// Veritabanlarını oluşturma ve migrasyonları uygulama
if (!useInMemoryDatabase)
{
    using (var scope = host.Services.CreateScope())
    {
        var sagaDbContext = scope.ServiceProvider.GetRequiredService<SagaStateDbContext>();
        sagaDbContext.Database.Migrate();

        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        appDbContext.Database.Migrate();
    }
}

// Uygulamayı çalıştırma
await host.RunAsync();

// Sürecin tamamlanmasını beklemek için bir süre bekleyin (örneğin 5 saniye)
await Task.Delay(5000);

// Uygulamayı durdurma
await host.StopAsync();

// Uygulama çalıştıktan sonra verileri almak için scope oluşturma
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var offers = await dbContext.Offers.ToListAsync();
    Console.WriteLine($"Total offers in DB: {offers.Count}");

    foreach (var offer in offers)
    {
        Console.WriteLine($"Offer ID: {offer.Id}, GhTur: {offer.GhTur}");
    }
}

Console.ReadKey();
