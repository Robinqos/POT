using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace SignalR
{
    public class SystemTimeSenderService : BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SystemTimeSenderService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                cancellationToken.ThrowIfCancellationRequested();
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", DateTime.Now.ToString());
            }
        }

    }
}