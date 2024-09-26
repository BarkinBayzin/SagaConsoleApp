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

        public async Task DeleteOpportunityAsync(Guid opportunityId)
        {
            await Task.CompletedTask;
        }
    }
}
