using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using System.Text.Json;

namespace SagaConsoleApp_v2.Services
{
    public class CrmIntegrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CrmIntegrationService> _logger;

        public CrmIntegrationService(ApplicationDbContext dbContext, ILogger<CrmIntegrationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<CrmOpportunity>> CreateUpgradeOpportunityAsync(string ghTur, DateTime dateTriggered)
        {
            try
            {
                _logger.LogInformation("[CrmIntegrationService] [CreateUpgradeOpportunityAsync] CRM Upgrade Opportunity oluşturuluyor, GhTur: {GhTur}", ghTur);

                var opportunity = CrmOpportunity.GetMockCrmOpportunity(ghTur); // Mock veri oluşturulacak

                await _dbContext.CrmOpportunities.AddAsync(opportunity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("[CrmIntegrationService] [CreateUpgradeOpportunityAsync] CRM Opportunity başarıyla oluşturuldu, OpportunityId: {OpportunityId}", opportunity.OpportunityId);
                return Result<CrmOpportunity>.Success(opportunity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CrmIntegrationService] [CreateUpgradeOpportunityAsync] CRM Opportunity oluşturulurken hata oluştu, Hata mesajı: {Message}", ex.Message);
                return Result<CrmOpportunity>.Error($"CRM fırsatı oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result> DeleteOpportunityAsync(Guid opportunityId)
        {
            _logger.LogInformation("[CrmIntegrationService] [DeleteOpportunityAsync] CRM fırsatı silme işlemi başlatılıyor, OpportunityId: {OpportunityId}", opportunityId);
            var opportunity = await _dbContext.CrmOpportunities.FirstOrDefaultAsync(o => o.OpportunityId == opportunityId);

            if (opportunity != null)
            {
                _dbContext.CrmOpportunities.Remove(opportunity);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("[CrmIntegrationService] [DeleteOpportunityAsync] CRM fırsatı başarıyla silindi, OpportunityId: {OpportunityId}", opportunityId);
                return Result.Success();
            }
            else
            {
                _logger.LogWarning("[CrmIntegrationService] [DeleteOpportunityAsync] Gönderilen OpportunityId'ye ait bir fırsat bulunamadı, silme işlemi gerçekleştirilemedi. OpportunityId: {OpportunityId}", opportunityId);
                return Result.Error("CRM fırsatı bulunamadı.");
            }
        }

        public async Task<Result<CrmOpportunity>> GetOpportunityByIdAsync(Guid opportunityId)
        {
            var opportunity = await _dbContext.CrmOpportunities.FirstOrDefaultAsync(o => o.OpportunityId == opportunityId);
            if (opportunity != null)
            {
                return Result<CrmOpportunity>.Success(opportunity);
            }
            else
            {
                return Result<CrmOpportunity>.Error("CRM fırsatı bulunamadı.");
            }
        }

        public async Task<Result<CrmOpportunity>> CheckExistingOpportunityAsync(string ghTur)
        {
            _logger.LogInformation("[CrmIntegrationService] [CheckExistingOpportunityAsync] CRM fırsat kontrol ediliyor, GhTur: {GhTur}", ghTur);

            var opportunity = await _dbContext.CrmOpportunities.FirstOrDefaultAsync(o => o.GhTur == ghTur);

            if (opportunity != null)
            {
                _logger.LogInformation("[CrmIntegrationService] [CheckExistingOpportunityAsync] CRM fırsat bulundu, OpportunityId: {OpportunityId}", opportunity.OpportunityId);
                return Result<CrmOpportunity>.Success(opportunity);
            }
            else
            {
                _logger.LogWarning("[CrmIntegrationService] [CheckExistingOpportunityAsync] CRM fırsatı bulunamadı, GhTur: {GhTur}", ghTur);
                return Result<CrmOpportunity>.Error("CRM fırsatı bulunamadı.");
            }
        }

        public async Task<Result> SubmitOfferToCrmAsync(Offer offer)
        {
            RestResponse response = null;
            try
            {
                CrmIntegrationOptions _crmIntegrationOptions = new();
                var senderUserId = "67311cb1-bf32-4912-91a8-36f2c16b82ec";
                var client = new RestClient();
                var request = new RestRequest(_crmIntegrationOptions.GetSubmitToCrmEndPoint(offer.GhTur), Method.Post);
                request.AddHeader(_crmIntegrationOptions.Header, senderUserId);
                request.AddHeader("Content-type", "application/json");
                request.AddJsonBody(new { OfferId = offer.Id, GhTur = offer.GhTur });

                response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("[CrmIntegrationService] [SubmitOfferToCrmAsync] Teklif CRM'e başarıyla gönderildi, OfferId: {OfferId}", offer.Id);
                    return Result.Success();
                }
                else
                {
                    var errorContent = JsonSerializer.Deserialize<string>(response.Content);
                    _logger.LogError("[CrmIntegrationService] [SubmitOfferToCrmAsync] Hata oluştu, Reason: {Reason}", errorContent);
                    return Result.Error(errorContent.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CrmIntegrationService] [SubmitOfferToCrmAsync] İstisna oluştu");
                return Result.Error($"CRM'e submit işlemi başarısız oldu: {ex.Message}");
            }
        }

    }

}
