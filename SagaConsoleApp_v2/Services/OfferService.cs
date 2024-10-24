using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;

namespace SagaConsoleApp_v2.Services
{
    public interface IOfferService
    {
        Task<Result<GhTurOfferCheckResult>> CheckGhTurAsync(string ghTur, bool checkOnlyInitial = false);
        Task<Result<Offer>> CreateUpgradeOfferAsync(Guid offerId, string ghTur, Guid opportunityId);
        Task<Result> DeleteOfferAsync(Guid offerId);
        Task ApproveOfferAsync(Guid offerId);
        Task RejectOfferAsync(Guid offerId);
        Task<Offer> GetOfferByIdAsync(Guid offerId);
        Task UpdateOfferAsync(Offer offer);
        Task<OfferWorkflowHistory> GetOfferWorkflowHistoryByIdAsync(Guid offerWorkflowHistoryId);
        Task<OfferWorkflowHistory> GetOrCreateOfferWorkflowHistoryAsync(Offer offer);
        Task UpdateOfferWorkflowHistoryAsync(OfferWorkflowHistory offerWorkflowHistory);
    }

    public class OfferService : IOfferService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OfferService> _logger;

        public OfferService(ApplicationDbContext dbContext, ILogger<OfferService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<GhTurOfferCheckResult>> CheckGhTurAsync(string ghTur, bool checkOnlyInitial = false)
        {
            _logger.LogInformation("[Offer Service] [CheckGhTurAsync] GHTur kontrol ediliyor: {@ghTur}", ghTur);
            var existingOffers = await _dbContext.Offers
                .Where(o => o.GhTur == ghTur && !o.IsDeleted)
                .ToListAsync();

            if (existingOffers.Any())
            {
                _logger.LogInformation("[Offer Service] [CheckGhTurAsync] GHTur başarıyla bulundu, offer getiriliyor: {@ghTur}", ghTur);

                var firstOffer = existingOffers.First();

                return Result<GhTurOfferCheckResult>.Success(new GhTurOfferCheckResult(
                    OfferId: firstOffer.Id,
                    IsNew: false,
                    IsOld: true,
                    IsMultipleOffers: existingOffers.Count > 1,
                    Model: new OfferGHTurSelectModel(
                        OfferId: firstOffer.Id,
                        GhTur: firstOffer.GhTur,
                        CreateDate: firstOffer.CreateDate,
                        Creator: firstOffer.Creator,
                        Description: firstOffer.Description
                    ),
                    UpgradeCurrency: null
                ));
            }

            _logger.LogInformation("[Offer Service] [CheckGhTurAsync] Offerlarda bulunamadı, Crm Opportunity kontrol ediliyor: {@ghTur}", ghTur);
            var crmOpportunity = await _dbContext.CrmOpportunities.FirstOrDefaultAsync(o => o.GhTur == ghTur);
            if (crmOpportunity is not null)
            {
                _logger.LogInformation("[Offer Service] [CheckGhTurAsync] Crm Opportunity başarıyla bulundu, getiriliyor: {@ghTur}", ghTur);
                return Result<GhTurOfferCheckResult>.Success(new GhTurOfferCheckResult(
                    OfferId: null,
                    IsNew: false,
                    IsOld: false,
                    IsMultipleOffers: false,
                    Model: null,
                    UpgradeCurrency: null
                ));
            }

            return Result<GhTurOfferCheckResult>.Error("GhTur sistemde veya CRM'de bulunamadı.");
        }

        public async Task<Result<Offer>> CreateUpgradeOfferAsync(Guid offerId, string ghTur, Guid opportunityId)
        {
            try
            {
                _logger.LogInformation("[Offer Service] [CreateUpgradeOfferAsync] Upgrade offer oluşturuluyor, initial ghTur : {@ghTur}", ghTur);
                var upgradeOffer = Offer.CreateUpgrade(
                    creator: "Sistem",
                    ghTur: ghTur,
                    createDate: DateTime.Now,
                    description: "Overcapacity Upgrade Teklifi");

                // Veritabanına ekleyin
                _dbContext.Offers.Add(upgradeOffer);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("[Offer Service] [CreateUpgradeOfferAsync] Upgrade offer başarıyla oluşturuldu, Upgrade Offer ID: {@id}", upgradeOffer.Id);
                return Result<Offer>.Success(upgradeOffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Offer Service] [CreateUpgradeOfferAsync] Upgrade offer oluşturulurken hata oluştu, Hata mesajı: {Message}", ex.Message);
                return Result<Offer>.Error($"Upgrade teklifi oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result> DeleteOfferAsync(Guid offerId)
        {
            _logger.LogInformation("[Offer Service] [DeleteOfferAsync] Offer silme işlemi başlatılıyor, offerId : {@offerId}", offerId);
            var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.Id == offerId);
            if (offer != null)
            {
                offer.MarkAsDelete();
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("[Offer Service] [DeleteOfferAsync] Offer silme işlemi başarılı.");
                return Result.Success();
            }
            else
            {
                _logger.LogError("[Offer Service] [DeleteOfferAsync] Gönderilen OfferId'ye ait bir offer bulunamadı, silme işlemi gerçekleştirilemedi. offerId : {@offerId}", offerId);
                return Result.Error("Offer bulunamadı.");
            }
        }

        public async Task ApproveOfferAsync(Guid offerId)
        {
            var offer = await _dbContext.Offers.FindAsync(offerId);
            if (offer == null)
            {
                _logger.LogError("Offer not found. OfferId: {OfferId}", offerId);
                throw new Exception("Offer not found");
            }
            
            offer.Status = Entities.Enums.WorkflowTaskStatus.Approved;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Offer approved. OfferId: {OfferId}", offerId);
        }

        public async Task RejectOfferAsync(Guid offerId)
        {
            var offer = await _dbContext.Offers.FindAsync(offerId);
            if (offer == null)
            {
                _logger.LogError("Offer not found. OfferId: {OfferId}", offerId);
                throw new Exception("Offer not found");
            }

            offer.Status = Entities.Enums.WorkflowTaskStatus.Rejected;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Offer rejected. OfferId: {OfferId}", offerId);
        }

        public async Task<Offer> GetOfferByIdAsync(Guid offerId)
        {
            return await _dbContext.Offers
                .Include(o => o.OfferWorkflowHistories)
                .FirstOrDefaultAsync(o => o.Id == offerId);
        }

        public async Task UpdateOfferAsync(Offer offer)
        {
            _dbContext.Offers.Update(offer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<OfferWorkflowHistory> GetOfferWorkflowHistoryByIdAsync(Guid offerWorkflowHistoryId)
        {
            return await _dbContext.OfferWorkflowHistories
                .Include(owh => owh.WorkflowInstance)
                .FirstOrDefaultAsync(owh => owh.Id == offerWorkflowHistoryId);
        }

        public async Task<OfferWorkflowHistory> GetOrCreateOfferWorkflowHistoryAsync(Offer offer)
        {
            var history = await _dbContext.OfferWorkflowHistories
                .Include(owh => owh.WorkflowInstance)
                .FirstOrDefaultAsync(h => h.OfferId == offer.Id);

            if (history == null)
            {
                // ApproverUser ve diğer gerekli bilgileri burada sağlayın
                ApproverUser? technical = null;
                ApproverUser? manager = null;
                ApproverUser? cm = null;
                ApproverUser? cfo = null;
                ApproverUser? ceo = null;
                ApproverUser? am = null;
                ApproverUser? cto = null;

                var workflowReasons = new List<WorkflowReason>();
                var workflowType = WorkflowType.OfferApproval;

                history = new OfferWorkflowHistory(offer, technical, manager, cm, cfo, ceo, am, cto, workflowReasons, workflowType);

                _dbContext.OfferWorkflowHistories.Add(history);
                await _dbContext.SaveChangesAsync();
            }

            return history;
        }

        public async Task UpdateOfferWorkflowHistoryAsync(OfferWorkflowHistory offerWorkflowHistory)
        {
            _dbContext.OfferWorkflowHistories.Update(offerWorkflowHistory);
            await _dbContext.SaveChangesAsync();
        }
    }

    public record GhTurOfferCheckResult(Guid? OfferId, bool IsOld, bool IsNew, bool IsMultipleOffers, OfferGHTurSelectModel? Model, Currency? UpgradeCurrency);
    public enum Currency
    {
        TRY,
        USD
        //,EUR
    }
    public record OfferGHTurSelectModel(Guid OfferId, string GhTur, DateTime CreateDate, string Creator, string Description);
}
