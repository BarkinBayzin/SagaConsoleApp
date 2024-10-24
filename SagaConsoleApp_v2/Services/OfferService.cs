using Ardalis.Result;
using SagaConsoleApp_v2.Data;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;

namespace SagaConsoleApp_v2.Services
{
    public class OfferService
    {
        private readonly List<Offer> _offers = new List<Offer>();
        private readonly ApplicationDbContext _dbContext;

        public OfferService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<GhTurOfferCheckResult>> CheckGhTurAsync(string ghTur, bool checkOnlyInitial = false)
        {
            var existingOffer = _offers.Where(o => o.GhTur == ghTur && !o.IsDeleted).ToList();

            if (existingOffer != null)
            {
                return Result<GhTurOfferCheckResult>.Success(new GhTurOfferCheckResult(
                    OfferId: existingOffer.First().Id,
                    IsNew: false,
                    IsOld: true,
                    IsMultipleOffers: existingOffer.Count > 1,
                    Model: new OfferGHTurSelectModel(
                        OfferId: existingOffer.First().Id,
                        GhTur: existingOffer.First().GhTur,
                        CreateDate: existingOffer.First().CreateDate,
                        Creator: existingOffer.First().Creator,
                        Description: existingOffer.First().Description
                    ),
                    UpgradeCurrency: null
                ));
            }

            var crmOpportunity = CrmOpportunity.GetMockCrmOpportunity();
            if (crmOpportunity.GhTur == ghTur)
            {
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

        public async Task<Result<Offer>> CreateUpgradeOfferAsync(Guid offerId, OvercapacityRequest request, CrmOpportunity opportunity)
        {
            try
            {
                var upgradeOffer = Offer.CreateUpgrade(
                    creator: "Sistem",
                    ghTur: request.GhTur,
                    createDate: DateTime.Now,
                    description: "Overcapacity Upgrade Teklifi");

                _offers.Add(upgradeOffer);

                return Result<Offer>.Success(upgradeOffer);
            }
            catch (Exception ex)
            {
                return Result<Offer>.Error($"Upgrade teklifi oluşturulurken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result> DeleteOfferAsync(Guid offerId)
        {
            var offer = _offers.FirstOrDefault(o => o.Id == offerId);
            if (offer != null)
            {
                offer.MarkAsDelete();
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            else
            {
                return Result.Error("Offer bulunamadı.");
            }
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
