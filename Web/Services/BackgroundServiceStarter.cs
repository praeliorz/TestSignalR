using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace TestSignalR.Web.Services
{
    public class BackgroundServiceStarter<T> : IHostedService where T : IHostedService
    {
        private readonly T backgroundService;

        public BackgroundServiceStarter(T backgroundService)
        {
            this.backgroundService = backgroundService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return backgroundService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return backgroundService.StopAsync(cancellationToken);
        }
    }
}