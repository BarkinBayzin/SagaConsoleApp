using MassTransit;
using Microsoft.Extensions.Hosting;
using SagaConsoleApp.Messages;

namespace SagaConsoleApp
{
    public class BusHostedService : IHostedService
    {
        private readonly IBusControl _busControl;

        public BusHostedService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Start the bus
            await _busControl.StartAsync(cancellationToken);

            // Simulate publishing the initial event
            var correlationId = Guid.NewGuid();
            var ghTur = "GHTUR-0010340-24";
            var dateTriggered = DateTime.Now;

            Console.WriteLine("[Main] Publishing OvercapacityRequestReceived event");

            await _busControl.Publish(new OverCapacityRequestReceived(correlationId, ghTur, dateTriggered));

            // Optionally, wait for the saga to complete
            await Task.Delay(5000, cancellationToken);

            // Stop the application
            Environment.Exit(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _busControl.StopAsync(cancellationToken);
        }
    }
}
