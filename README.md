# **Proje Dokümantasyonu: Overcapacity Saga Uygulaması**

## **1. Giriş**

Bu proje, **MassTransit** kütüphanesi kullanılarak geliştirilen bir **Saga Tasarım Deseni** uygulamasıdır. Amaç, sistemde oluşabilecek aşırı kapasite durumlarını etkin bir şekilde yönetmek ve ilgili iş süreçlerini otomatikleştirmektir. Proje, mikroservis mimarisi üzerine inşa edilmiş olup, dağıtık işlemlerin tutarlı ve hatasız bir şekilde yürütülmesini hedeflemektedir.

## **2. Projenin Amacı ve Kapsamı**

- **Aşırı Kapasite Taleplerinin Yönetimi**: Kullanıcı veya sistem tarafından tetiklenen aşırı kapasite taleplerinin otomatik olarak işlenmesi.
- **CRM Entegrasyonu**: Fırsatların CRM sisteminde oluşturulması, güncellenmesi ve silinmesi işlemlerinin entegrasyonu.
- **Teklif Oluşturma ve Yönetimi**: Yeni fırsatlar için tekliflerin oluşturulması ve mevcut tekliflerin yönetilmesi.
- **Bildirim Sistemi**: İlgili kullanıcılara ve birimlere bildirim e-postalarının gönderilmesi.
- **Telafi Mekanizmaları**: Hata durumlarında sistemin önceki tutarlı durumuna geri dönebilmesi için telafi işlemlerinin uygulanması.

## **3. Mimari ve Teknolojiler**

### **3.1. Mimari**

- **Mikroservis Mimarisi**: Uygulama, belirli işlevleri yerine getiren bağımsız servislerden oluşur.
- **Saga Tasarım Deseni**: Dağıtık işlemlerin koordinasyonu ve tutarlılığı için kullanılır.
- **Event-Driven Architecture (EDA)**: Sistem, olaylara dayalı olarak çalışır ve mesajlaşma ile iletişim kurar.

### **3.2. Teknolojiler**

- **.NET Core 6.0 veya Üstü**: Uygulama geliştirme platformu.
- **MassTransit**: Mesajlaşma ve saga yönetimi için kullanılan kütüphane.
- **RabbitMQ veya InMemory Transport**: Mesaj kuyruğu ve iletişim altyapısı.
- **Dependency Injection (DI)**: Bağımlılık yönetimi için kullanılır.
- **Logging (Serilog, NLog vb.)**: Uygulama içi loglama işlemleri için kullanılır.
- **RestSharp veya HttpClient**: HTTP tabanlı API çağrıları için kullanılır.

## **4. Bileşenler ve Katmanlar**

### **4.1. Saga**

- **OvercapacitySaga**: Aşırı kapasite işlemlerini yöneten ana saga sınıfı.
- **OvercapacitySagaState**: Saga'nın durum bilgisini tutan sınıf.

### **4.2. Mesajlar (Events ve Commands)**

- **Events**: Sistem içinde gerçekleşen olayları temsil eder.
  - **OvercapacityRequestReceived**
  - **OpportunityCreated**
  - **OpportunityCreationFailed**
  - **UpgradeOfferCreated**
  - **UpgradeOfferCreationFailed**
  - **NotificationEmailSent**
  - **NotificationEmailFailed**
  - **OfferDeleted**
  - **OfferDeletionFailed**
  - **OpportunityDeleted**
  - **OpportunityDeletionFailed**
  - **StartWorkflow**
  - **CrmSubmitFailed**

- **Commands**: Belirli bir işlemin yapılmasını talep eden mesajlardır.
  - **CreateOpportunity**
  - **CreateUpgradeOffer**
  - **SendNotificationEmail**
  - **DeleteOffer**
  - **DeleteOpportunity**

### **4.3. Tüketiciler (Consumers)**

- **CreateOpportunityConsumer**: CRM'de fırsat oluşturma işlemini gerçekleştirir.
- **CreateUpgradeOfferConsumer**: Upgrade teklifi oluşturur.
- **SendNotificationEmailConsumer**: Bildirim e-postasını gönderir.
- **DeleteOfferConsumer**: Teklifi siler.
- **DeleteOpportunityConsumer**: CRM'deki fırsatı siler.
- **OvercapacityRequestConsumer**: Aşırı kapasite taleplerini işleyen tüketici (isteğe bağlı).

### **4.4. Servisler**

- **CrmIntegrationService**: CRM sistemine entegrasyonu sağlar.
- **OfferService**: Teklif oluşturma, güncelleme ve silme işlemlerini yönetir.
- **EmailService**: E-posta gönderme işlemlerini yönetir.

### **4.5. Yapılandırma Dosyaları**

- **appsettings.json**: Uygulama yapılandırma ayarlarını içerir.
- **Program.cs**: Uygulamanın başlangıç noktası ve servislerin yapılandırılması.

## **5. İş Akışı ve Süreçler**

### **5.1. Genel Akış**

1. **OvercapacityRequestReceived** mesajı alınır ve saga başlatılır.
2. Saga, **CreateOpportunity** komutunu gönderir.
3. **CreateOpportunityConsumer** komutu işleyerek CRM'de fırsat oluşturur.
   - Başarılı ise **OpportunityCreated** eventi yayınlanır.
   - Başarısız ise **OpportunityCreationFailed** eventi yayınlanır.
4. Saga, **OpportunityCreated** eventini alır ve **CreateUpgradeOffer** komutunu gönderir.
5. **CreateUpgradeOfferConsumer** komutu işleyerek teklif oluşturur.
   - Başarılı ise **UpgradeOfferCreated** eventi yayınlanır.
   - Başarısız ise **UpgradeOfferCreationFailed** eventi yayınlanır.
6. Saga, **UpgradeOfferCreated** eventini alır ve **SendNotificationEmail** komutunu gönderir.
7. **SendNotificationEmailConsumer** komutu işleyerek e-posta gönderir.
   - Başarılı ise **NotificationEmailSent** eventi yayınlanır.
   - Başarısız ise **NotificationEmailFailed** eventi yayınlanır.
8. Saga, **NotificationEmailSent** eventini alır ve **StartWorkflow** eventini yayınlayarak tamamlanır.
9. Hata durumlarında saga, telafi işlemlerini başlatarak ilgili silme işlemlerini gerçekleştirir.

### **5.2. Durumlar ve Geçişler**

- **Initial**: Saga'nın başlangıç durumu.
- **Validated**: Fırsat oluşturma işleminin başlatıldığı durum.
- **OpportunityCreatedState**: Fırsatın oluşturulduğu ve teklif oluşturma işleminin başlatıldığı durum.
- **OfferCreatedState**: Teklifin oluşturulduğu ve e-posta gönderme işleminin başlatıldığı durum.
- **EmailSentState**: E-postanın gönderildiği durum.
- **CompensationInProgress**: Hata durumlarında telafi işlemlerinin yapıldığı durum.
- **Completed**: Saga'nın başarıyla tamamlandığı durum.
- **Failed**: Saga'nın başarısız olduğu durum.

## **6. Örnek Kod Parçaları**

### **6.1. OvercapacitySaga**

```csharp
public class OvercapacitySaga : MassTransitStateMachine<OvercapacitySagaState>
{
    // Durum tanımlamaları
    public State Validated { get; private set; }
    public State OpportunityCreatedState { get; private set; }
    public State OfferCreatedState { get; private set; }
    public State CompensationInProgress { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    // Event tanımlamaları
    public Event<OvercapacityRequestReceived> OvercapacityRequestReceived { get; private set; }
    public Event<OpportunityCreated> OpportunityCreated { get; private set; }
    public Event<OpportunityCreationFailed> OpportunityCreationFailed { get; private set; }
    public Event<UpgradeOfferCreated> UpgradeOfferCreated { get; private set; }
    public Event<UpgradeOfferCreationFailed> UpgradeOfferCreationFailed { get; private set; }
    public Event<NotificationEmailSent> NotificationEmailSent { get; private set; }
    public Event<NotificationEmailFailed> NotificationEmailFailed { get; private set; }
    // Diğer eventler...

    public OvercapacitySaga(ILogger<OvercapacitySaga> logger)
    {
        // Durum ve eventlerin tanımlanması
        // Durum geçişlerinin ve event işleyicilerinin tanımlanması
    }
}
```

### **6.2. CreateOpportunityConsumer**

```csharp
public class CreateOpportunityConsumer : IConsumer<CreateOpportunity>
{
    private readonly CrmIntegrationService _crmIntegrationService;
    private readonly ILogger<CreateOpportunityConsumer> _logger;

    public CreateOpportunityConsumer(CrmIntegrationService crmIntegrationService, ILogger<CreateOpportunityConsumer> logger)
    {
        _crmIntegrationService = crmIntegrationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateOpportunity> context)
    {
        _logger.LogInformation("CreateOpportunity komutu alındı.");

        var result = await _crmIntegrationService.CreateOpportunityAsync(context.Message.GhTur);

        if (result.IsSuccess)
        {
            await context.Publish(new OpportunityCreated
            {
                CorrelationId = context.Message.CorrelationId,
                CrmOpportunityId = result.Value
            });
        }
        else
        {
            await context.Publish(new OpportunityCreationFailed
            {
                CorrelationId = context.Message.CorrelationId,
                Reason = result.Errors.FirstOrDefault()
            });
        }
    }
}
```

### **6.3. SendNotificationEmailConsumer**

```csharp
public class SendNotificationEmailConsumer : IConsumer<UpgradeOfferCreated>
{
    private readonly EmailService _emailService;
    private readonly ILogger<SendNotificationEmailConsumer> _logger;

    public SendNotificationEmailConsumer(EmailService emailService, ILogger<SendNotificationEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpgradeOfferCreated> context)
    {
        _logger.LogInformation("SendNotificationEmail komutu alındı.");

        var result = await _emailService.SendEmailAsync(context.Message.OfferId);

        if (result.IsSuccess)
        {
            await context.Publish(new NotificationEmailSent
            {
                CorrelationId = context.Message.CorrelationId,
                OfferId = context.Message.OfferId
            });
        }
        else
        {
            await context.Publish(new NotificationEmailFailed
            {
                CorrelationId = context.Message.CorrelationId,
                OfferId = context.Message.OfferId,
                Reason = result.Errors.FirstOrDefault()
            });
        }
    }
}
```

## **7. Yapılandırma ve Başlatma**

### **7.1. Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Servislerin kaydedilmesi
services.AddScoped<CrmIntegrationService>();
services.AddScoped<OfferService>();
services.AddScoped<EmailService>();

// MassTransit yapılandırması
services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OvercapacitySaga, OvercapacitySagaState>()
        .InMemoryRepository();

    x.AddConsumer<CreateOpportunityConsumer>(cfg =>
    {
        cfg.Endpoint(e => e.Name = "create-opportunity");
    });

    x.AddConsumer<CreateUpgradeOfferConsumer>(cfg =>
    {
        cfg.Endpoint(e => e.Name = "create-upgrade-offer");
    });

    x.AddConsumer<SendNotificationEmailConsumer>(cfg =>
    {
        cfg.Endpoint(e => e.Name = "send-notification-email");
    });

    // Diğer tüketiciler...

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    // veya assembly üzerinden consumer okuyabilirsiniz
    cfg.AddConsumers(typeof(Program).Assembly);

    // Endpoint mappingi için kısa yol kebab case kullanımı
    cfg.SetKebabCaseEndpointNameFormatter();
});

var app = builder.Build();

app.Run();
```

### **7.2. appsettings.json**

```json
{
  "CrmIntegrationOptions": {
    "Header": "YourHeaderValue",
    "Host": "https://your-crm-host.com",
    "Users": "UsersEndpoint",
    // Diğer yapılandırmalar...
  },
  // Diğer ayarlar...
  // Şu anda options pattern henüz kullanılmıyor projede 
}
```

## **8. Bağımlılık Yönetimi ve Enjeksiyon**

- **Dependency Injection (DI)** kullanılarak servislerin ve tüketicilerin bağımlılıkları yönetilir.
- **IOptions<T>** kullanılarak yapılandırma ayarlarına erişim sağlanır.
- Tüm servisler **AddScoped** veya **AddSingleton** ile DI konteynerine kaydedilir.

## **9. Loglama ve Hata Yönetimi**

- **ILogger** arayüzü kullanılarak kapsamlı bir loglama yapılmıştır.
- Hata durumlarında uygun loglama ve telafi mekanizmaları uygulanır.
- Saga içinde hata durumlarında **Failed** veya **CompensationInProgress** durumlarına geçiş yapılır.

## **10. Compansation(Telafi) Mekanizmaları**

- Hata oluştuğunda, saga ilgili telafi işlemlerini başlatır.
- Örneğin, teklif oluşturma başarısız olursa, oluşturulmuş fırsat silinir.
- Telafi işlemleri için **DeleteOffer** ve **DeleteOpportunity** komutları ve ilgili tüketiciler kullanılır.

## **11. Sonuç ve Öneriler**

- Proje, aşırı kapasite durumlarının otomatik ve tutarlı bir şekilde yönetilmesini sağlar.
- **Saga Tasarım Deseni** ve **MassTransit** kullanımıyla dağıtık işlemlerin koordinasyonu etkin bir şekilde sağlanmıştır.
- İleride, uygulamanın performansını artırmak için mesajlaşma altyapısı olarak RabbitMQ kullanılabilir projenin ihtiyacına göre bu projede olduğu gibi EF Core InMemoryDb tercih edilebilir.
- Loglama ve izleme araçlarıyla sistemin izlenebilirliği artırılabilir.

---
