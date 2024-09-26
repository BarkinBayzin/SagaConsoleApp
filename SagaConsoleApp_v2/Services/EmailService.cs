using Ardalis.Result;
using SagaConsoleApp_v2.Entities;
using SagaConsoleApp_v2.Messages;

namespace SagaConsoleApp_v2.Services
{
    public class EmailService
    {
        public async Task<Result> SendOvercapacityNotificationAsync(Offer upgradeOffer, OvercapacityRequest request)
        {
            await Task.CompletedTask;
            return Result.Success();
        }
    }
}
