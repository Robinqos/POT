using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TypedClientInterfaces;

namespace SignalR
{
    public class StronglyTypedChatHub : Hub<IChatClient>
    {
        public Task SendMessage(string user, string message)
        {
            return Clients.All.ReceiveMessage(user, message);
        }
    }
}