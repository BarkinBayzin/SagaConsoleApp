using Ardalis.Result;
using SagaConsoleApp_v2.Entities;

namespace SagaConsoleApp_v2.Services
{
    public class CrmIntegrationService
    {
        public async Task<Result<CrmOpportunity>> CreateUpgradeOpportunityAsync(string ghTur, DateTime dateTriggered)
        {
            var opportunity = CrmOpportunity.GetMockCrmOpportunity();
            return Result<CrmOpportunity>.Success(opportunity);
        }

        public async Task<Result> DeleteOpportunityAsync(Guid opportunityId)
        {
            // Simülasyon amaçlı bir işlem
            await Task.Delay(100);

            bool isDeleted = true; // Silme işleminin başarılı veya başarısız olması durumu

            if (isDeleted)
            {
                return Result.Success();
            }
            else
            {
                return Result.Error("CRM fırsatı silinemedi.");
            }
        }
    }
}
